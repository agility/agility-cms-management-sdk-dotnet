using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// Content items: read, list, save, delete, and workflow (publish, approve, and so on).
/// </summary>
/// <remarks>
/// Saves and workflow operations run as batches. By default each method waits for its batch and
/// returns a <see cref="BatchResult"/>; pass <c>waitForBatch: false</c> to get the batch ID straight away.
/// A save always lands in Staging, even for a published item: publish it afterwards for the change to go live.
/// </remarks>
public sealed class ContentClient
{
    private readonly ManagementConnection _connection;
    private readonly string _guid;
    private readonly BatchesClient _batches;

    internal ContentClient(ManagementConnection connection, string guid, BatchesClient batches)
    {
        _connection = connection;
        _guid = guid;
        _batches = batches;
    }

    /// <summary>Gets a content item. <c>GET {locale}/item/{contentID}</c></summary>
    /// <param name="locale">The locale code, e.g. <c>en-us</c>.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentItem> GetContentItemAsync(string locale, int contentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/item/{contentId}"),
            RequestKind.Read, ManagementJsonContext.Default.ContentItem, cancellationToken: cancellationToken);
    }

    /// <summary>Gets several content items in one request. <c>GET {locale}/items?ids=</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentIds">The content IDs.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentItem>> GetContentItemsByIdAsync(string locale, IEnumerable<int> contentIds, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(contentIds);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/items", new Query().Add("ids", contentIds)),
            RequestKind.Read, ManagementJsonContext.Default.ListContentItem, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lists the items in a container, with optional filtering and sorting. <c>POST {locale}/list/{referenceName}</c>
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="referenceName">The container's reference name.</param>
    /// <param name="filter">Filters to apply (states, dates, field values, free text). <see langword="null"/> lists everything.</param>
    /// <param name="take">Items per page.</param>
    /// <param name="skip">How many items to skip.</param>
    /// <param name="showDeleted">Include deleted items.</param>
    /// <param name="fields">Comma-separated field names to return, to keep responses small.</param>
    /// <param name="sortField">The field to sort by.</param>
    /// <param name="sortDirection"><c>asc</c> or <c>desc</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <remarks>
    /// The 1.x SDK called a hidden <c>GET</c> version of this route that ignored its filter. This is the
    /// documented operation.
    /// </remarks>
    public Task<ContentList> GetContentListAsync(
        string locale, string referenceName, ContentListFilterModel? filter = null, int? take = null, int? skip = null,
        bool? showDeleted = null, string? fields = null, string? sortField = null, string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceName);
        var body = filter ?? new ContentListFilterModel();
        // A POST, but only a read: safe to retry.
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"{locale}/list/{referenceName}", new Query()
                .Add("take", take).Add("skip", skip).Add("showDeleted", showDeleted).Add("fields", fields)
                .Add("sortField", sortField).Add("sortDirection", sortDirection)),
            RequestKind.Read, ManagementJsonContext.Default.ContentList,
            ManagementConnection.Json(body, ManagementJsonContext.Default.ContentListFilterModel), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Saves a content item: creates it when <c>ContentID</c> is 0 or less, otherwise replaces it.
    /// <c>POST {locale}/item</c>
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="item">
    /// The whole item. The API replaces the stored item with what you send, so read it first, change what you
    /// need, and send it all back: a partial item loses the fields you left out.
    /// </param>
    /// <param name="waitForBatch">Wait for the save to be processed. <see cref="BatchResult.ItemId"/> is then the item's content ID.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> SaveContentItemAsync(string locale, ContentItem item, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(item);
        var batchId = await _connection.SendAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"{locale}/item"),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32,
            ManagementConnection.Json(item, ManagementJsonContext.Default.ContentItem), cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, "save content item", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Saves several content items in one batch. <c>POST {locale}/item/multi</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="items">The whole items, as for <see cref="SaveContentItemAsync"/>.</param>
    /// <param name="waitForBatch">Wait for the saves. <see cref="BatchResult.ItemIds"/> then lists the content IDs in order.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> SaveContentItemsAsync(string locale, IReadOnlyList<ContentItem> items, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(items);
        var list = items as List<ContentItem> ?? [.. items];
        var batchId = await _connection.SendAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"{locale}/item/multi"),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32,
            ManagementConnection.Json(list, ManagementJsonContext.Default.ListContentItem), cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, "save content items", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Deletes a content item. <c>DELETE {locale}/item/{id}</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="comments">A comment for the item's history.</param>
    /// <param name="waitForBatch">Wait for the delete to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> DeleteContentItemAsync(string locale, int contentId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var batchId = await _connection.SendAsync(HttpMethod.Delete,
            _connection.InstanceUri(_guid, $"{locale}/item/{contentId}", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, "delete content item", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Publishes a content item. <c>GET {locale}/item/{contentID}/publish</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="comments">A comment for the item's history.</param>
    /// <param name="waitForBatch">Wait for the publish to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> PublishContentItemAsync(string locale, int contentId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, contentId, "publish", comments, waitForBatch, cancellationToken);

    /// <summary>Unpublishes a content item. <c>GET {locale}/item/{contentID}/unpublish</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="comments">A comment for the item's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> UnpublishContentItemAsync(string locale, int contentId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, contentId, "unpublish", comments, waitForBatch, cancellationToken);

    /// <summary>Approves a content item. <c>GET {locale}/item/{contentID}/approve</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="comments">A comment for the item's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> ApproveContentItemAsync(string locale, int contentId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, contentId, "approve", comments, waitForBatch, cancellationToken);

    /// <summary>Declines a content item. <c>GET {locale}/item/{contentID}/decline</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="comments">A comment for the item's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> DeclineContentItemAsync(string locale, int contentId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, contentId, "decline", comments, waitForBatch, cancellationToken);

    /// <summary>Requests approval for a content item. <c>GET {locale}/item/{contentID}/request-approval</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="comments">A comment for the item's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> RequestApprovalContentItemAsync(string locale, int contentId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, contentId, "request-approval", comments, waitForBatch, cancellationToken);

    /// <summary>
    /// Runs one workflow operation on many content items in a single batch. <c>POST {locale}/item/batch-workflow</c>
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentIds">The content IDs.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> BatchWorkflowContentItemsAsync(
        string locale, IEnumerable<int> contentIds, WorkflowOperationType operation, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(contentIds);
        var batchId = await _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"{locale}/item/batch-workflow", new Query()
                .Add("contentIDs", contentIds).Add("operation", operation.ToApiValue())),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, $"content {operation}", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists the items that publishing this content item would also publish (linked content, nested lists).
    /// <c>GET {locale}/item/{contentID}/cascade-items</c>
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<CascadeItem> GetCascadeItemsAsync(string locale, int contentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/item/{contentId}/cascade-items"),
            RequestKind.Read, ManagementJsonContext.Default.CascadeItem, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Publishes a content item and the items it depends on. <c>POST {locale}/item/{contentID}/publish-cascade</c>.
    /// The API may create more than one batch; wait for each with <see cref="BatchesClient.WaitForBatchAsync"/>.
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="comments">A comment for the items' history.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<BatchCreateResult> PublishContentItemCascadeAsync(string locale, int contentId, string? comments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"{locale}/item/{contentId}/publish-cascade", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.BatchCreateResult, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a content item's comments. <c>GET {locale}/item/{contentID}/comments</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="take">Comments per page.</param>
    /// <param name="skip">How many comments to skip.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ItemCommentsResponse> GetContentItemCommentsAsync(string locale, int contentId, int? take = null, int? skip = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/item/{contentId}/comments", new Query().Add("take", take).Add("skip", skip)),
            RequestKind.Read, ManagementJsonContext.Default.ItemCommentsResponse, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a content item's version history. <c>GET {locale}/item/{contentID}/history</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="contentId">The content ID.</param>
    /// <param name="take">Versions per page.</param>
    /// <param name="skip">How many versions to skip.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentItemHistoryResponse> GetContentItemHistoryAsync(string locale, int contentId, int? take = null, int? skip = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/item/{contentId}/history", new Query().Add("take", take).Add("skip", skip)),
            RequestKind.Read, ManagementJsonContext.Default.ContentItemHistoryResponse, cancellationToken: cancellationToken);
    }

    // Workflow operations are GETs in the API, but they change state: never retried.
    private async Task<BatchResult> WorkflowAsync(string locale, int contentId, string operation, string? comments, bool waitForBatch, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var batchId = await _connection.SendAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/item/{contentId}/{operation}", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, $"content item {operation}", waitForBatch, cancellationToken).ConfigureAwait(false);
    }
}
