using agility.models;
using RestSharp;
using System.Text;
using System.Text.Json;

namespace management.api.sdk
{
    public class PageMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        BatchMethods _batchMethods = null;
        private Options _options = null;
        public PageMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);
            _batchMethods = new BatchMethods(_options);
        }

        /// <summary>
        /// Method to retrieve sitemap for a locale.
        /// </summary>
        /// <returns>A collection of Sitemap object.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Sitemap?>> GetSiteMap()
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/sitemap");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive sitemap for locale {_options.locale}. Additional Details: {response.Result.Content}");
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
        /// Method to Get a Page.
        /// </summary>
        /// <param name="pageID">The id of the requested page.</param>
        /// <returns>An object of a PageItem.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<PageItem?> GetPage(int? pageID)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page/{pageID}");
                var response = client.ExecuteAsync(request, Method.Get);

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
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> PublishPage(int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page/{pageID}/publish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to publish the page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);
                return batchID;
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
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> UnPublishPage(int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page/{pageID}/unpublish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to un-publish page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);
                return batchID;
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
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> DeletePage(int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page/{pageID}?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Delete);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);
                return batchID;
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
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> ApprovePage(int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page/{pageID}/approve?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to approve page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);
                return batchID;
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
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> DeclinePage(int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page/{pageID}/decline?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to decline page for id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);
                return batchID;
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
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> PageRequestApproval(int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page/{pageID}/request-approval?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to request approval for page with id: {pageID}. Additional Details: {response.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(response.Result.Content);
                return batchID;
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
        /// <param name="parentPageID">The id of the parent page.</param>
        /// <param name="placeBeforePageItemID">The id of the page before the page.</param>
        /// <returns>Returns a string pageID of the requested page.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string?> SavePage(PageItem? pageItem, int? parentPageID = -1, int? placeBeforePageItemID = -1)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/page?parentPageID={parentPageID}&placeBeforePageItemID={placeBeforePageItemID}");
                request.AddJsonBody(pageItem, "application/json");
                var id = client.ExecuteAsync(request, Method.Post);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save page. Additional Details: {id.Result.Content}");
                }
                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();

                foreach (var item in batch.Items)
                {
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
                }
                if (!string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (response.Length > 0)
                    {
                        response.Append(",");
                    }
                    response.Append($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }
                return response.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Batch?> GetBatchObject(int? id)
        {
            try
            {
                var response = await _batchMethods.GetBatch(id);
                return response;
            }
            catch
            {
                throw;
            }
        }
    }
}
