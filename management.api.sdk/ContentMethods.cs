using agility.models;
using RestSharp;
using System.Text;
using System.Text.Json;

namespace management.api.sdk
{
    public class ContentMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        BatchMethods _batchMethods = null;
        public ContentMethods(string? guid)
        {
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(guid);
            _batchMethods = new BatchMethods(guid);
        }
        public async Task<string> GetContentItem(string? locale, int? contentID)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/{contentID}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
            
        }

        public async Task<string> PublishContent(string? locale, int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/{contentID}/publish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
           
        }

        public async Task<string> UnPublishContent(string? locale, int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/{contentID}/unpublish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
          
        }

        public async Task<string> ContentRequestApproval(string? locale, int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/{contentID}/request-approval?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {

                throw;
            }
            
        }

        public async Task<string> ApproveContent(string? locale, int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/{contentID}/approve?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
            
        }

        public async Task<string> DeclineContent(string? locale, int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/{contentID}/decline?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
           
        }

        public async Task<string> SaveContentItem(string? locale, ContentItem? contentItem)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item");
                request.AddJsonBody(contentItem, "application/json");
                var id = client.ExecuteAsync(request, Method.Post).Result.Content;
                var batchID = JsonSerializer.Deserialize<int>(id);
                
                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID != Int32.MaxValue)
                    {
                        response.Append(item.ItemID);
                    }
                    //else
                    //{
                    //    response.Append($"Error record found for batch item {item.BatchItemID}. Additional details on error {batch.ErrorData}");
                    //}
                    seperator = ",";
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

        public async Task<string> SaveContentItems(string? locale, List<ContentItem?> contentItems)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/multi");
                request.AddJsonBody(contentItems, "application/json");
                var id = client.ExecuteAsync(request, Method.Post).Result.Content;
                var batchID = JsonSerializer.Deserialize<int>(id);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));
                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID != Int32.MaxValue)
                    {
                        response.Append(item.ItemID);
                    }
                    //else
                    //{
                    //    response.Append($"Error record found for batch item {item.BatchItemID}. Additional details on error {batch.ErrorData}");
                    //}
                    seperator = ",";
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

        public async Task<string> DeleteContent(string? locale, int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/item/{contentID}?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Delete).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
            
        }
        public async Task<string?> GetContentItems(string? locale, string? referenceName,
           string? filter = null,
           string? fields = null,
           string? sortDirection = null,
           string? sortField = null,
           int? take = 50,
           int? skip = 0)
        {
            try
            {
                var request = new RestRequest($"/{locale}/list/{referenceName}?filter={filter}&fields={fields}&sortDirection={sortDirection}&sortField={sortField}&take={take}&skip={skip}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }
    }
}
