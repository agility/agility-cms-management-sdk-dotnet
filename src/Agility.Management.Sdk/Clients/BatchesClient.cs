using System.Net;
using System.Text.Json;
using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// Batches: the API's unit of asynchronous work. Saves and workflow operations return a batch ID, and
/// the change happens when the batch is processed.
/// </summary>
public sealed class BatchesClient
{
    private readonly ManagementConnection _connection;
    private readonly string _guid;

    internal BatchesClient(ManagementConnection connection, string guid)
    {
        _connection = connection;
        _guid = guid;
    }

    /// <summary>Gets a batch. <c>GET batch/{id}</c></summary>
    /// <param name="batchId">The batch ID.</param>
    /// <param name="expandItems">Include the batch's items.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Batch> GetBatchAsync(int batchId, bool expandItems = true, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"batch/{batchId}", new Query().Add("expandItems", expandItems)),
            RequestKind.Read, ManagementJsonContext.Default.Batch, cancellationToken: cancellationToken);

    /// <summary>
    /// Creates a batch of workflow operations on pages and content items. <c>POST batch</c>
    /// </summary>
    /// <param name="request">The operation and the items to apply it to.</param>
    /// <param name="processNow">Queue the batch for processing now rather than leaving it as a draft.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<BatchCreateResult> CreateBatchAsync(CreateBatchWithItemsRequest request, bool? processNow = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        // Unset, the API would read the operation as Publish (its first value): refuse rather than publish by accident.
        if (request.Operation is null)
            throw new ArgumentException($"Set {nameof(CreateBatchWithItemsRequest.Operation)}.", nameof(request));
        if (request.Items is null || request.Items.Count == 0 || request.Items.Any(i => i.ItemType is null))
            throw new ArgumentException($"Add at least one item, each with an {nameof(AddBatchItemRequest.ItemType)}.", nameof(request));
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"batch", new Query().Add("processNow", processNow)),
            RequestKind.Write, ManagementJsonContext.Default.BatchCreateResult,
            ManagementConnection.Json(request, ManagementJsonContext.Default.CreateBatchWithItemsRequest), cancellationToken: cancellationToken);
    }

    /// <summary>Publishes every item in an existing batch. <c>POST batch/{id}/publish</c></summary>
    /// <param name="batchId">The batch ID.</param>
    /// <param name="waitForBatch">Wait for the resulting batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> PublishBatchAsync(int batchId, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        RunBatchOperationAsync(batchId, "publish", waitForBatch, cancellationToken);

    /// <summary>Unpublishes every item in an existing batch. <c>POST batch/{id}/unpublish</c></summary>
    /// <param name="batchId">The batch ID.</param>
    /// <param name="waitForBatch">Wait for the resulting batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> UnpublishBatchAsync(int batchId, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        RunBatchOperationAsync(batchId, "unpublish", waitForBatch, cancellationToken);

    /// <summary>Approves every item in an existing batch. <c>POST batch/{id}/approve</c></summary>
    /// <param name="batchId">The batch ID.</param>
    /// <param name="waitForBatch">Wait for the resulting batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> ApproveBatchAsync(int batchId, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        RunBatchOperationAsync(batchId, "approve", waitForBatch, cancellationToken);

    /// <summary>Declines every item in an existing batch. <c>POST batch/{id}/decline</c></summary>
    /// <param name="batchId">The batch ID.</param>
    /// <param name="waitForBatch">Wait for the resulting batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> DeclineBatchAsync(int batchId, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        RunBatchOperationAsync(batchId, "decline", waitForBatch, cancellationToken);

    /// <summary>Requests approval for every item in an existing batch. <c>POST batch/{id}/request-approval</c></summary>
    /// <param name="batchId">The batch ID.</param>
    /// <param name="waitForBatch">Wait for the resulting batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> RequestApprovalBatchAsync(int batchId, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        RunBatchOperationAsync(batchId, "request-approval", waitForBatch, cancellationToken);

    /// <summary>
    /// Waits until a batch is processed, checking every <see cref="BatchPollingOptions.Interval"/>.
    /// </summary>
    /// <param name="batchId">The batch ID.</param>
    /// <param name="cancellationToken">Stops waiting. The batch keeps running on the server.</param>
    /// <returns>The processed batch, with its items.</returns>
    /// <exception cref="AgilityBatchException">The batch was processed but some items failed, or it was aborted or deleted.</exception>
    /// <exception cref="AgilityBatchTimeoutException">The batch didn't finish within <see cref="BatchPollingOptions.Timeout"/>.</exception>
    public async Task<Batch> WaitForBatchAsync(int batchId, CancellationToken cancellationToken = default)
    {
        var polling = _connection.Options.BatchPolling;
        var time = _connection.Time;
        var started = time.GetTimestamp();
        Batch? last = null;

        while (true)
        {
            try
            {
                last = await GetBatchAsync(batchId, expandItems: true, cancellationToken).ConfigureAwait(false);
            }
            catch (AgilityManagementException ex) when (ex.StatusCode == HttpStatusCode.NotFound && time.GetElapsedTime(started) < polling.NotFoundGracePeriod)
            {
                // A new batch ID can 404 for a moment before the batch exists.
            }

            if (last is not null && IsAborted(last))
                throw new AgilityBatchException($"Batch {batchId} was aborted.", batchId, last);
            switch (last?.BatchState)
            {
                case BatchState.Processed:
                    ThrowIfFailed(batchId, last);
                    return last;
                case BatchState.Deleted:
                    throw new AgilityBatchException($"Batch {batchId} was deleted before it was processed.", batchId, last);
            }

            var elapsed = time.GetElapsedTime(started);
            if (elapsed >= polling.Timeout)
                throw new AgilityBatchTimeoutException(
                    $"Batch {batchId} wasn't processed within {polling.Timeout} (last state: {last?.BatchState.ToString() ?? "not found"}). " +
                    "It keeps running on the server; check it in the Batches page of the Content Manager or with GetBatchAsync.",
                    batchId, last, elapsed);

            var wait = polling.Interval < polling.Timeout - elapsed ? polling.Interval : polling.Timeout - elapsed;
            await Task.Delay(wait, time, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Turns the batch ID an operation returned into a <see cref="BatchResult"/>, waiting if asked.</summary>
    internal async Task<BatchResult> CompleteAsync(int? batchId, string operation, bool waitForBatch, CancellationToken cancellationToken)
    {
        if (batchId is not int id || id <= 0)
            throw new AgilityManagementException($"The API didn't return a batch ID for {operation} (got {batchId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "null"}).");
        if (!waitForBatch) return new BatchResult(id, null);
        var batch = await WaitForBatchAsync(id, cancellationToken).ConfigureAwait(false);
        return new BatchResult(id, batch);
    }

    private async Task<BatchResult> RunBatchOperationAsync(int batchId, string operation, bool waitForBatch, CancellationToken cancellationToken)
    {
        var id = await _connection.SendAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"batch/{batchId}/{operation}"),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await CompleteAsync(id, $"batch {operation}", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    internal static void ThrowIfFailed(int batchId, Batch batch)
    {
        if (IsAborted(batch))
            throw new AgilityBatchException($"Batch {batchId} was aborted.", batchId, batch);

        var failed = (batch.Items ?? []).Where(i => !string.IsNullOrWhiteSpace(i.ErrorMessage)).ToList();
        var count = Math.Max(failed.Count, SummaryFailureCount(batch.ErrorData));
        if (count == 0) return;

        var details = string.Join("; ", failed.Take(3).Select(i => $"item {i.ItemID}: {ItemErrorText(i.ErrorMessage!)}"));
        throw new AgilityBatchException(
            $"Batch {batchId} was processed with {count} failed item(s)" + (details.Length > 0 ? $": {details}" : "."), batchId, batch);
    }

    // The processor marks a cancelled batch with AbortYN, or with this ErrorData, and may stop short of Processed.
    private static bool IsAborted(Batch batch) =>
        batch.AbortYN == true || string.Equals(batch.ErrorData?.Trim(), "Batch aborted.", StringComparison.OrdinalIgnoreCase);

    private static int SummaryFailureCount(string? errorData)
    {
        if (string.IsNullOrWhiteSpace(errorData) || !errorData.TrimStart().StartsWith('{')) return 0;
        try
        {
            using var doc = JsonDocument.Parse(errorData);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("FailureCount", out var f)
                && f.ValueKind == JsonValueKind.Number && f.TryGetInt32(out var n) ? n : 0;
        }
        catch (JsonException)
        {
            return 0;
        }
    }

    // Item errors are a BatchErrorInfo JSON object; show its Message when there is one.
    private static string ItemErrorText(string errorMessage)
    {
        if (errorMessage.TrimStart().StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(errorMessage);
                if (doc.RootElement.TryGetProperty("Message", out var m) && m.ValueKind == JsonValueKind.String)
                    return m.GetString()!;
            }
            catch (JsonException)
            {
                // Show it as sent.
            }
        }
        return errorMessage;
    }
}
