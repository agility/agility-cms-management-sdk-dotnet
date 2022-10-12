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
        public PageMethods(string? guid)
        {
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(guid);
            _batchMethods = new BatchMethods(guid);
        }
        public async Task<string?> GetSiteMap(string? locale)
        {
            try
            {
                var request = new RestRequest($"/{locale}/sitemap");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetPage(string? locale, int? pageID)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> PublishPage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/publish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> UnPublishPage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/unpublish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeletePage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Delete).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> ApprovePage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/approve?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeclinePage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/decline?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> PageRequestApproval(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/request-approval?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> SavePage(string? locale, PageItem? pageItem, int? parentPageID = -1, int? placeBeforePageItemID = -1)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page?parentPageID={parentPageID}&placeBeforePageItemID={placeBeforePageItemID}");
                request.AddJsonBody(pageItem, "application/json");
                var id = client.ExecuteAsync(request, Method.Post).Result.Content;
                var batchID = JsonSerializer.Deserialize<int>(id);

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
            catch
            {
                throw;
            }
        }

        public async Task<Batch?> GetBatchObject(int? id)
        {
            try
            {
                var response = await _batchMethods.GetBatch(id);
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var batch = JsonSerializer.Deserialize<Batch>(response, options);
                return batch;
            }
            catch
            {
                throw;
            }
        }
    }
}
