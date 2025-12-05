using agility.models;
using System.Text;
using System.Text.Json;

namespace management.api.sdk
{
    public class PageMethods
    {
        BatchMethods _batchMethods;
        private Options _options;
        ExecuteMethods executeMethods;
        public PageMethods(Options options)
        {
            _options = options;
            _batchMethods = new BatchMethods(options);
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Method to retrieve sitemap for a locale.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <returns>A collection of Sitemap object.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Sitemap?>> GetSiteMap(string guid, string locale)
        {
            try
            {
                var apiPath = $"/{locale}/sitemap";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive sitemap for locale {locale}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var sitemap = JsonSerializer.Deserialize<List<Sitemap>>(response.Result.Content, options);

                return sitemap;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to retrieve page templates for the website.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="includeModuleZones">To include zones in the result.</param>
        /// <param name="searchFilter">Filter search results.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>

        public async Task<List<PageModel>?> GetPageTemplates(string guid, string locale, bool includeModuleZones, string? searchFilter = null)
        {
            try
            {
                var apiPath = $"/{locale}/page/templates?includeModuleZones={includeModuleZones}&searchFilter={searchFilter}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive Page Templates for locale {locale}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var pageTemplates = JsonSerializer.Deserialize<List<PageModel>>(response.Result.Content, options);

                return pageTemplates;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to get a page template.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="pageTemplateId">The id of the requested page template.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>

        public async Task<PageModel?> GetPageTemplate(string guid, string locale, int? pageTemplateId)
        {
            try
            {
                var apiPath = $"/{locale}/page/template/{pageTemplateId}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive Page Template for Page Template ID {pageTemplateId}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var pageTemplate = JsonSerializer.Deserialize<PageModel>(response.Result.Content, options);

                return pageTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to retrieve page template by name
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="templateName">Name of the requested page template.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>

        public async Task<PageModel?> GetPageTemplateByName(string guid, string locale, string? templateName)
        {
            try
            {
                var apiPath = $"/{locale}/page/template/{templateName}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive Page Template for Name {templateName}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var pageTemplate = JsonSerializer.Deserialize<PageModel>(response.Result.Content, options);

                return pageTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Method to delete a page template.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="pageTemplateId">The id of the requested page template.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string?> DeletePageTemplate(string guid, string locale, int? pageTemplateId)
        {
            try
            {
                var apiPath = $"/{locale}/page/template/{pageTemplateId}";
                var response = executeMethods.ExecuteDelete(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to Delete Page Template for Page Template ID {pageTemplateId}. Additional Details: {response.Result.Content}");
                }
                
                return response.Result.Content;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Method to get Page Pemplate items for a pageTemplateId
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="id">The pageTemplateId for the reuqested page template.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<ContentSectionDefinition?>> GetPageItemTemplates(string guid, string locale, int? id)
        {
            try
            {
                var apiPath = $"/{locale}/page/template/items/{id}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive page item templates for template id {id}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var pageItemTemplate = JsonSerializer.Deserialize<List<ContentSectionDefinition>>(response.Result.Content, options);

                return pageItemTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to save a page template.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="pageModel">The object of the requested page template.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<PageModel?> SavePageTemplate(string guid, string locale, PageModel pageModel)
        {
            try
            {
                var apiPath = $"/{locale}/page/template";
                var response = executeMethods.ExecutePost(apiPath, guid, pageModel, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to Save Page Template. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var pageTemplate = JsonSerializer.Deserialize<PageModel>(response.Result.Content, options);

                return pageTemplate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to Get a Page.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <returns>An object of a PageItem.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<PageItem?> GetPage(int? pageID, string guid, string locale)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive page for pageID {pageID}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var page = JsonSerializer.Deserialize<PageItem>(response.Result.Content, options);

                return page;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to publish a page from batch.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> PublishPage(int? pageID, string guid, string locale, string? comments = null, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}/publish?comments={comments}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to publish the page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);

                if (returnBatchId)
                {
                    return batchID;
                }

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdPage = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdPage = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to publish Page.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to publish Page.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdPage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to Unpublish a page from batch.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> UnPublishPage(int? pageID, string guid, string locale, string? comments = null, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}/unpublish?comments={comments}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to un-publish page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);

                if (returnBatchId)
                {
                    return batchID;
                }

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdPage = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdPage = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to un-publish Page.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to un-publish Page.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdPage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Method to delete a page from batch.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> DeletePage(int? pageID, string guid, string locale, string? comments = null, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}?comments={comments}";

                var response = executeMethods.ExecuteDelete(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);

                if (returnBatchId)
                {
                    return batchID;
                }

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdPage = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdPage = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to delete Page.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to delete Page.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdPage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to Approve a page from batch.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> ApprovePage(int? pageID, string guid, string locale, string? comments = null, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}/approve?comments={comments}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to approve page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);

                if (returnBatchId)
                {
                    return batchID;
                }

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdPage = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdPage = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to approved Page.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to approved Page.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdPage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to Decline a page from batch.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> DeclinePage(int? pageID, string guid, string locale, string? comments = null, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}/decline?comments={comments}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to decline page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);

                if (returnBatchId)
                {
                    return batchID;
                }

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdPage = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdPage = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to decline Page.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to decline Page.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdPage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to Request approval for a page from batch.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> PageRequestApproval(int? pageID, string guid, string locale, string? comments = null, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}/request-approval?comments={comments}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to request approval for page with id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);

                if (returnBatchId)
                {
                    return batchID;
                }

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdPage = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdPage = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to request approval for Page.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to request approval for Page.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdPage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to save a page from batch.
        /// </summary>
        /// <param name="pageItem">The object of PageItem class for the requested Page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="parentPageID">The id of the parent page.</param>
        /// <param name="placeBeforePageItemID">The id of the page before the page.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> SavePage(PageItem? pageItem, string guid, string locale, int? parentPageID = -1, int? placeBeforePageItemID = -1, int? pageIDInOtherLocale = -1, string? otherLocale = null, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/{locale}/page?parentPageID={parentPageID}&placeBeforePageItemID={placeBeforePageItemID}";
                if(pageIDInOtherLocale > 0)
                {
                    apiPath += $"&pageIDInOtherLocale={pageIDInOtherLocale}&otherLocale={otherLocale}";
                }

                var id = executeMethods.ExecutePost(apiPath, guid, pageItem, _options.token);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save page. Additional Details: {id.Result.Content}");
                }
                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                if (returnBatchId)
                {
                    return batchID;
                }

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdPage = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdPage = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to create Page.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to create Page.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdPage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to retrieve a batch object.
        /// </summary>
        /// <param name="id">The id of the requested batch.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of the Batch class.</returns>
        public async Task<Batch?> GetBatchObject(int? id, string guid)
        {
            try
            {
                var response = await _batchMethods.GetBatch(id, guid);
                return response;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Method to retrieve page history.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <returns>A collection of PageHistory objects.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<PageHistory>?> GetPageHistory(int? pageID, string guid, string locale)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}/history";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve page history for pageID {pageID}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var history = JsonSerializer.Deserialize<List<PageHistory>>(response.Result.Content, options);

                return history;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to retrieve page comments.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <returns>An ItemComments object.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<ItemComments?> GetPageComments(int? pageID, string guid, string locale)
        {
            try
            {
                var apiPath = $"/{locale}/page/{pageID}/comments";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve page comments for pageID {pageID}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var comments = JsonSerializer.Deserialize<ItemComments>(response.Result.Content, options);

                return comments;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
