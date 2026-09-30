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
    private readonly string _guid;
    private readonly BatchesClient _batches;

    internal PagesClient(ManagementConnection connection, string guid, BatchesClient batches)
    {
        _connection = connection;
        _guid = guid;
        _batches = batches;
    }

    /// <summary>Gets the sitemap for a locale. <c>GET {locale}/sitemap</c></summary>
    /// <param name="locale">The locale code, e.g. <c>en-us</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<Sitemap>> GetSitemapAsync(string locale, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/sitemap"),
            RequestKind.Read, ManagementJsonContext.Default.ListSitemap, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page. <c>GET {locale}/page/{id}</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageItem> GetPageAsync(string locale, int pageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/page/{pageId}"),
            RequestKind.Read, ManagementJsonContext.Default.PageItem, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Saves a page: creates it when <c>PageID</c> is 0 or less, otherwise replaces it. <c>POST {locale}/page</c>
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="page">The whole page, as for content items: read it, change it, send it all back.</param>
    /// <param name="parentPageId">The parent page, for a new page. Omitted: the API's default (the root).</param>
    /// <param name="placeBeforePageId">Put the page before this sibling. Omitted: last.</param>
    /// <param name="otherLocale">With <paramref name="pageIdInOtherLocale"/>: the locale of the page this one translates.</param>
    /// <param name="pageIdInOtherLocale">The ID of the same page in <paramref name="otherLocale"/>.</param>
    /// <param name="linkExistingComponents">Link the page's components to existing content instead of copying it.</param>
    /// <param name="waitForBatch">Wait for the save. <see cref="BatchResult.ItemId"/> is then the page ID.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> SavePageAsync(
        string locale, PageItem page, int? parentPageId = null, int? placeBeforePageId = null, string? otherLocale = null,
        int? pageIdInOtherLocale = null, bool? linkExistingComponents = null, bool waitForBatch = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(page);
        var batchId = await _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"{locale}/page", new Query()
                .Add("parentPageID", parentPageId).Add("placeBeforePageItemID", placeBeforePageId)
                .Add("otherLocale", otherLocale).Add("pageIDInOtherLocale", pageIdInOtherLocale)
                .Add("linkExistingComponents", linkExistingComponents)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32,
            ManagementConnection.Json(page, ManagementJsonContext.Default.PageItem), cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, "save page", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Deletes a page. <c>DELETE {locale}/page/{id}</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the delete to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> DeletePageAsync(string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var batchId = await _connection.SendAsync(HttpMethod.Delete,
            _connection.InstanceUri(_guid, $"{locale}/page/{pageId}", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, "delete page", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Publishes a page. <c>GET {locale}/page/{id}/publish</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the publish to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> PublishPageAsync(string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, pageId, "publish", comments, waitForBatch, cancellationToken);

    /// <summary>Unpublishes a page. <c>GET {locale}/page/{id}/unpublish</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> UnpublishPageAsync(string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, pageId, "unpublish", comments, waitForBatch, cancellationToken);

    /// <summary>Approves a page. <c>GET {locale}/page/{id}/approve</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> ApprovePageAsync(string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, pageId, "approve", comments, waitForBatch, cancellationToken);

    /// <summary>Declines a page. <c>GET {locale}/page/{id}/decline</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> DeclinePageAsync(string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, pageId, "decline", comments, waitForBatch, cancellationToken);

    /// <summary>Requests approval for a page. <c>GET {locale}/page/{id}/request-approval</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the page's history.</param>
    /// <param name="waitForBatch">Wait for the operation to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public Task<BatchResult> RequestApprovalPageAsync(string locale, int pageId, string? comments = null, bool waitForBatch = true, CancellationToken cancellationToken = default) =>
        WorkflowAsync(locale, pageId, "request-approval", comments, waitForBatch, cancellationToken);

    /// <summary>Runs one workflow operation on many pages in a single batch. <c>POST {locale}/page/batch-workflow</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageIds">The page IDs.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="waitForBatch">Wait for the batch to be processed.</param>
    /// <param name="cancellationToken">Cancels the request or the wait.</param>
    public async Task<BatchResult> BatchWorkflowPagesAsync(
        string locale, IEnumerable<int> pageIds, WorkflowOperationType operation, bool waitForBatch = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(pageIds);
        var ids = pageIds.ToList();
        if (ids.Count == 0) throw new ArgumentException("Pass at least one page ID.", nameof(pageIds));
        var batchId = await _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"{locale}/page/batch-workflow", new Query()
                .Add("pageIDs", ids).Add("operation", operation.ToApiValue())),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, $"page {operation}", waitForBatch, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists what publishing this page would also publish (its components' content, nested lists).
    /// <c>GET {locale}/page/{id}/cascade-items</c>
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<CascadeItem> GetCascadeItemsAsync(string locale, int pageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/page/{pageId}/cascade-items"),
            RequestKind.Read, ManagementJsonContext.Default.CascadeItem, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Publishes a page and the content it depends on. <c>POST {locale}/page/{id}/publish-cascade</c>.
    /// The API may create more than one batch; wait for each with <see cref="BatchesClient.WaitForBatchAsync"/>.
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="comments">A comment for the items' history.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<BatchCreateResult> PublishPageCascadeAsync(string locale, int pageId, string? comments = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"{locale}/page/{pageId}/publish-cascade", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.BatchCreateResult, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page's comments. <c>GET {locale}/page/{id}/comments</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="take">Comments per page.</param>
    /// <param name="skip">How many comments to skip.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ItemCommentsResponse> GetPageCommentsAsync(string locale, int pageId, int? take = null, int? skip = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/page/{pageId}/comments", new Query().Add("take", take).Add("skip", skip)),
            RequestKind.Read, ManagementJsonContext.Default.ItemCommentsResponse, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page's version history. <c>GET {locale}/page/{id}/history</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageId">The page ID.</param>
    /// <param name="take">Versions per page.</param>
    /// <param name="skip">How many versions to skip.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageHistoryResponse> GetPageHistoryAsync(string locale, int pageId, int? take = null, int? skip = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/page/{pageId}/history", new Query().Add("take", take).Add("skip", skip)),
            RequestKind.Read, ManagementJsonContext.Default.PageHistoryResponse, cancellationToken: cancellationToken);
    }

    /// <summary>Lists page templates (page models). <c>GET {locale}/page/templates</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="includeModuleZones">Include each template's zones (<c>ContentSectionDefinitions</c>).</param>
    /// <param name="searchFilter">Only templates whose name contains this text.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<PageModel>> GetPageTemplatesAsync(string locale, bool includeModuleZones = false, string? searchFilter = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/page/templates", new Query()
                .Add("includeModuleZones", includeModuleZones).Add("searchFilter", searchFilter)),
            RequestKind.Read, ManagementJsonContext.Default.ListPageModel, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a page template with every zone and each zone's default components.
    /// <c>GET {locale}/page/template/{pageTemplateId}</c>
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageTemplateId">The page template ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageModel> GetPageTemplateAsync(string locale, int pageTemplateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/page/template/{pageTemplateId}"),
            RequestKind.Read, ManagementJsonContext.Default.PageModel, cancellationToken: cancellationToken);
    }

    /// <summary>Gets a page template by name. <c>GET {locale}/page/template/{templateName}</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="templateName">The template name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PageModel> GetPageTemplateByNameAsync(string locale, string templateName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/page/template/{templateName}"),
            RequestKind.Read, ManagementJsonContext.Default.PageModel, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a page template's zones. <c>GET {locale}/page/template/items/{id}</c>. This route always returns
    /// <c>DefaultModules</c> as empty: to read or round-trip a zone's default components, use
    /// <see cref="GetPageTemplateAsync"/>.
    /// </summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageTemplateId">The page template ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentSectionDefinition>> GetPageTemplateZonesAsync(string locale, int pageTemplateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"{locale}/page/template/items/{pageTemplateId}"),
            RequestKind.Read, ManagementJsonContext.Default.ListContentSectionDefinition, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates or updates a page template. <c>POST {locale}/page/template</c>
    /// </summary>
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
    public Task<PageModel> SavePageTemplateAsync(string locale, PageModel pageTemplate, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(pageTemplate);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"{locale}/page/template"),
            RequestKind.Write, ManagementJsonContext.Default.PageModel,
            ManagementConnection.Json(pageTemplate, ManagementJsonContext.Default.PageModel), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a page template. <c>DELETE {locale}/page/template/{pageTemplateId}</c></summary>
    /// <param name="locale">The locale code.</param>
    /// <param name="pageTemplateId">The page template ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeletePageTemplateAsync(string locale, int pageTemplateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        return _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(_guid, $"{locale}/page/template/{pageTemplateId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
    }

    // Workflow operations are GETs in the API, but they change state: never retried.
    private async Task<BatchResult> WorkflowAsync(string locale, int pageId, string operation, string? comments, bool waitForBatch, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        var batchId = await _connection.SendAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"{locale}/page/{pageId}/{operation}", new Query().Add("comments", comments)),
            RequestKind.Write, ManagementJsonContext.Default.NullableInt32, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await _batches.CompleteAsync(batchId, $"page {operation}", waitForBatch, cancellationToken).ConfigureAwait(false);
    }
}
