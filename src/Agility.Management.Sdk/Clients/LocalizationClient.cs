using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// Copies pages and content into other locales: <b>initialize</b> copies them as they are, <b>translate</b>
/// also machine-translates them. Each call runs as a batch.
/// </summary>
public sealed class LocalizationClient
{
    private readonly ManagementConnection _connection;
    private readonly string _guid;
    private readonly BatchesClient _batches;

    internal LocalizationClient(ManagementConnection connection, string guid, BatchesClient batches)
    {
        _connection = connection;
        _guid = guid;
        _batches = batches;
    }

    /// <summary>Copies pages into other locales. <c>POST initialize/page</c></summary>
    /// <param name="request">The pages, the source locale and the target locales.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> InitializePagesAsync(InitializePageRequest request, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        SendAsync(_connection.InstanceUri(_guid, $"initialize/page"), request, ManagementJsonContext.Default.InitializePageRequest, "initialize pages", waitForBatch, cancellationToken);

    /// <summary>Copies content lists into other locales. <c>POST initialize/contentlist</c></summary>
    /// <param name="request">The lists, the source locale and the target locales.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> InitializeContentListsAsync(InitializeContentListRequest request, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        SendAsync(_connection.InstanceUri(_guid, $"initialize/contentlist"), request, ManagementJsonContext.Default.InitializeContentListRequest, "initialize content lists", waitForBatch, cancellationToken);

    /// <summary>Copies content items into other locales. <c>POST initialize/content</c></summary>
    /// <param name="request">The items, the source locale and the target locales.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> InitializeContentItemsAsync(InitializeContentRequest request, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        SendAsync(_connection.InstanceUri(_guid, $"initialize/content"), request, ManagementJsonContext.Default.InitializeContentRequest, "initialize content", waitForBatch, cancellationToken);

    /// <summary>Translates pages into other locales. <c>POST translate/page</c></summary>
    /// <param name="request">The pages, the source locale and the target locales.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> TranslatePagesAsync(TranslatePageRequest request, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        SendAsync(_connection.InstanceUri(_guid, $"translate/page"), request, ManagementJsonContext.Default.TranslatePageRequest, "translate pages", waitForBatch, cancellationToken);

    /// <summary>Translates content lists into other locales. <c>POST translate/contentlist</c></summary>
    /// <param name="request">The lists, the source locale and the target locales.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> TranslateContentListsAsync(TranslateContentListRequest request, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        SendAsync(_connection.InstanceUri(_guid, $"translate/contentlist"), request, ManagementJsonContext.Default.TranslateContentListRequest, "translate content lists", waitForBatch, cancellationToken);

    /// <summary>Translates content items into other locales. <c>POST translate/content</c></summary>
    /// <param name="request">The items, the source locale and the target locales.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> TranslateContentItemsAsync(TranslateContentRequest request, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        SendAsync(_connection.InstanceUri(_guid, $"translate/content"), request, ManagementJsonContext.Default.TranslateContentRequest, "translate content", waitForBatch, cancellationToken);

    private async Task<BatchResult> SendAsync<T>(
        Uri uri, T request, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> type, string operation,
        bool waitForBatch, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var batchId = await _connection.SendAsync(HttpMethod.Post, uri,
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32,
            ManagementConnection.Json(request, type), cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, operation, waitForBatch, cancellationToken).ConfigureAwait(false);
    }
}
