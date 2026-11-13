using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// Pages, the sitemap, and page templates (page models).
/// </summary>
/// <remarks>
/// Page saves and workflow operations run as batches, like content saves: see <see cref="ContentClient"/>.
/// </remarks>
public sealed class PagesClient
{
    private readonly ManagementConnection _connection;
    private readonly BatchesClient _batches;

    internal PagesClient(ManagementConnection connection, BatchesClient batches)
    {
        _connection = connection;
        _batches = batches;
    }

    /// <summary>Gets the sitemap for a locale. <c>GET {locale}/sitemap</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code, e.g. <c>en-us</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<Sitemap>> GetSitemapAsync(string instanceGuid, string locale, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"{locale}/sitemap"),
            RequestKind.Read, ManagementJsonContext.Default.ListSitemap, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page. <c>GET {locale}/page/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageItem> GetPageAsync(string instanceGuid, string locale, int pageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}"),
            RequestKind.Read, ManagementJsonContext.Default.PageItem, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Saves a page: creates it when <c>PageID</c> is 0 or less, otherwise replaces it. <c>POST {locale}/page</c>
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="page">The whole page, as for content items: read it, change it, send it all back.</param>
    /// <param name="options">Where to put a new page and how to link it. <see langword="null"/>: the API's defaults.</param>
    /// <param name="waitForBatch">Wait for the save. <see cref="BatchResult.ItemId"/> is then the page ID.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> SavePageAsync(
        string instanceGuid, string locale, PageItem page, SavePageOptions? options = null, bool waitForBatch = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(page);
        var o = options ?? new SavePageOptions();
        var batchId = await _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"{locale}/page", new Query()
                .Add("parentPageID", o.ParentPageId).Add("placeBeforePageItemID", o.PlaceBeforePageId)
                .Add("otherLocale", o.OtherLocale).Add("pageIDInOtherLocale", o.PageIdInOtherLocale)
                .Add("linkExistingComponents", o.LinkExistingComponents)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32,
            ManagementConnection.Json(page, ManagementJsonContext.Default.PageItem), cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(instanceGuid, batchId, "save page", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Deletes a page. <c>DELETE {locale}/page/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the delete to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> DeletePageAsync(string instanceGuid, string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var batchId = await _connection.SendAsync(HttpMethod.Delete,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(instanceGuid, batchId, "delete page", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Publishes a page. <c>GET {locale}/page/{id}/publish</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the publish to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> PublishPageAsync(string instanceGuid, string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(instanceGuid, locale, pageId, "publish", comments, waitForBatch, cancellationToken);

    /// <summary>Unpublishes a page. <c>GET {locale}/page/{id}/unpublish</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> UnpublishPageAsync(string instanceGuid, string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(instanceGuid, locale, pageId, "unpublish", comments, waitForBatch, cancellationToken);

    /// <summary>Approves a page. <c>GET {locale}/page/{id}/approve</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> ApprovePageAsync(string instanceGuid, string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(instanceGuid, locale, pageId, "approve", comments, waitForBatch, cancellationToken);

    /// <summary>Declines a page. <c>GET {locale}/page/{id}/decline</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> DeclinePageAsync(string instanceGuid, string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(instanceGuid, locale, pageId, "decline", comments, waitForBatch, cancellationToken);

    /// <summary>Requests approval for a page. <c>GET {locale}/page/{id}/request-approval</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> RequestApprovalPageAsync(string instanceGuid, string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(instanceGuid, locale, pageId, "request-approval", comments, waitForBatch, cancellationToken);

    /// <summary>Runs one workflow operation on many pages in a single batch. <c>POST {locale}/page/batch-workflow</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageIds">The page IDs.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> BatchWorkflowPagesAsync(string instanceGuid, string locale, IEnumerable<int> pageIds, WorkflowOperationType operation, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(pageIds);
        var ids = pageIds.ToList();
        if (ids.Count == 0) throw new ArgumentException("Pass at least one page ID.", nameof(pageIds));
        var batchId = await _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/batch-workflow", new Query()
                .Add("pageIDs", ids).Add("operation", operation.ToApiValue())),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(instanceGuid, batchId, $"page {operation}", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists what publishing this page would also publish (its components' content, nested lists).
    /// <c>GET {locale}/page/{id}/cascade-items</c>
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<CascadeItem> GetCascadeItemsAsync(string instanceGuid, string locale, int pageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}/cascade-items"),
            RequestKind.Read, ManagementJsonContext.Default.CascadeItem, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Publishes a page and the content it depends on. <c>POST {locale}/page/{id}/publish-cascade</c>.
    /// The API may create more than one batch; wait for each with <see cref="BatchesClient.WaitForBatchAsync"/>.
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the items' history.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<BatchCreateResult> PublishPageCascadeAsync(string instanceGuid, string locale, int pageId, string? comments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}/publish-cascade", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.BatchCreateResult, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Moves a page to a new place in its sitemap: under a parent page, before one of that parent's pages.
    /// <c>POST {locale}/page/{id}/move</c>
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code to look the pages up in. The move applies to every locale.</param>
    /// <param name="pageId">The page to move.</param>
    /// <param name="parentPageId">The new parent page; omit for the root of the sitemap.</param>
    /// <param name="placeBeforePageId">
    /// The page to place it before, which must already be under the new parent; omit to place it last.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    /// The page's parent and latest version, whether it moved (false when it was already in that place, and nothing
    /// changed), and the parent's pages in their new order.
    /// </returns>
    /// <remarks>
    /// The same move as dragging the page in the page tree. A page's place is shared by all locales: the move creates
    /// a new version and puts the page in staging in every locale it exists in, so publish it for the live sitemap to
    /// show the new place. Its child pages move with it. One request; no batch. It writes, so it isn't retried.
    /// A page, parent or page to place before that isn't found is a 404; a move that isn't allowed (under itself or
    /// one of its own pages, a page to place before under another parent, another page with the same name or a
    /// dynamic page already at that level) is a 400.
    /// </remarks>
    public Task<PageMoveResult> MovePageAsync(
        string instanceGuid, string locale, int pageId, int? parentPageId = null, int? placeBeforePageId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}/move",
                new Query().Add("parentPageID", parentPageId ?? -1).Add("placeBeforePageID", placeBeforePageId ?? -1)),
            RequestKind.Write, ManagementJsonContext.Default.PageMoveResult, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page's comments. <c>GET {locale}/page/{id}/comments</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="take">Comments per page.</param>
    /// <param name="skip">How many comments to skip.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ItemCommentsResponse> GetPageCommentsAsync(string instanceGuid, string locale, int pageId, int? take = null, int? skip = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}/comments", new Query().Add("take", take).Add("skip", skip)),
            RequestKind.Read, ManagementJsonContext.Default.ItemCommentsResponse, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page's version history. <c>GET {locale}/page/{id}/history</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="take">Versions per page.</param>
    /// <param name="skip">How many versions to skip.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageHistoryResponse> GetPageHistoryAsync(string instanceGuid, string locale, int pageId, int? take = null, int? skip = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}/history", new Query().Add("take", take).Add("skip", skip)),
            RequestKind.Read, ManagementJsonContext.Default.PageHistoryResponse, cancellationToken: cancellationToken);
    }

    /// <summary>Lists page templates (page models). <c>GET {locale}/page/templates</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="includeModuleZones">Include each template's zones (<c>ContentSectionDefinitions</c>).</param>
    /// <param name="searchFilter">Only templates whose name contains this text.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<PageModel>> GetPageTemplatesAsync(string instanceGuid, string locale, bool includeModuleZones = false, string? searchFilter = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/templates", new Query()
                .Add("includeModuleZones", includeModuleZones).Add("searchFilter", searchFilter)),
            RequestKind.Read, ManagementJsonContext.Default.ListPageModel, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a page template with every zone and each zone's default components.
    /// <c>GET {locale}/page/template/{pageTemplateId}</c>
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageTemplateId">The page template ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageModel> GetPageTemplateAsync(string instanceGuid, string locale, int pageTemplateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"{locale}/page/template/{pageTemplateId}"),
            RequestKind.Read, ManagementJsonContext.Default.PageModel, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page template by name. <c>GET {locale}/page/template/{templateName}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="templateName">The template name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageModel> GetPageTemplateByNameAsync(string instanceGuid, string locale, string templateName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"{locale}/page/template/{templateName}"),
            RequestKind.Read, ManagementJsonContext.Default.PageModel, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a page template's zones. <c>GET {locale}/page/template/items/{id}</c>. This route always returns
    /// <c>DefaultModules</c> as empty: to read or round-trip a zone's default components, use
    /// <see cref="GetPageTemplateAsync"/>.
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageTemplateId">The page template ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentSectionDefinition>> GetPageTemplateZonesAsync(string instanceGuid, string locale, int pageTemplateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"{locale}/page/template/items/{pageTemplateId}"),
            RequestKind.Read, ManagementJsonContext.Default.ListContentSectionDefinition, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates or updates a page template. <c>POST {locale}/page/template</c>
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageTemplate">
    /// The template. How the API reads it:
    /// <list type="bullet">
    /// <item><c>ContentSectionDefinitions</c> is the <b>complete</b> zone list: zones you leave out are deleted,
    /// and their components disappear from every page that uses the template. Leave it <see langword="null"/>
    /// to keep the zones as they are.</item>
    /// <item>A zone's <c>DefaultModules</c> replaces that zone's default components, and an empty list clears
    /// them. Leave it <see langword="null"/> to keep them as they are.</item>
    /// </list>
    /// Reading a template with <see cref="GetPageTemplateAsync"/> and saving it back keeps everything.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageModel> SavePageTemplateAsync(string instanceGuid, string locale, PageModel pageTemplate, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(pageTemplate);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(instanceGuid, $"{locale}/page/template"),
            RequestKind.Write, ManagementJsonContext.Default.PageModel,
            ManagementConnection.Json(pageTemplate, ManagementJsonContext.Default.PageModel), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a page template. <c>DELETE {locale}/page/template/{pageTemplateId}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageTemplateId">The page template ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeletePageTemplateAsync(string instanceGuid, string locale, int pageTemplateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(instanceGuid, $"{locale}/page/template/{pageTemplateId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
    }

    // Workflow operations are GETs in the API, but they change state: never retried.
    private async Task<BatchResult> WorkflowAsync(string instanceGuid, string locale, int pageId, string operation, string? comments, bool waitForBatch, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var batchId = await _connection.SendAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"{locale}/page/{pageId}/{operation}", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(instanceGuid, batchId, $"page {operation}", waitForBatch, cancellationToken).ConfigureAwait(false);
    }
}
