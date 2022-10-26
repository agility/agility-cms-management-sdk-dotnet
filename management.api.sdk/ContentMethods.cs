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
        private Options _options = null;
        public ContentMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);
            _batchMethods = new BatchMethods(_options);
        }
        public async Task<ContentItem> GetContentItem(int? contentID)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/{contentID}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the content for id: {contentID}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var contentItem = JsonSerializer.Deserialize<ContentItem>(response.Result.Content, options);

                return contentItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<string?> PublishContent(int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/{contentID}/publish?comments={comments}");
                var id = client.ExecuteAsync(request, Method.Get);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to publish the content for id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(id.Result.Content);
  
                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
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
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<string?> UnPublishContent(int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/{contentID}/unpublish?comments={comments}");
                var id = client.ExecuteAsync(request, Method.Get);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to un-publish the content for id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
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
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<string?> ContentRequestApproval(int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/{contentID}/request-approval?comments={comments}");
                var id = client.ExecuteAsync(request, Method.Get);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to request for approval for the content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
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
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<string?> ApproveContent(int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/{contentID}/approve?comments={comments}");
                var id = client.ExecuteAsync(request, Method.Get);
                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to approve content for the content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string?> DeclineContent(int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/{contentID}/decline?comments={comments}");
                var id = client.ExecuteAsync(request, Method.Get);
                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to decline content for the content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
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
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<string> SaveContentItem(ContentItem? contentItem)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item");
                request.AddJsonBody(contentItem, "application/json");
                var id = client.ExecuteAsync(request, Method.Post);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save content. Additional Details: {id.Result.Content}");
                }
                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);
                
                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
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

        public async Task<List<string?>> SaveContentItems(List<ContentItem?> contentItems)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/multi");
                request.AddJsonBody(contentItems, "application/json");
                var id = client.ExecuteAsync(request, Method.Post);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save contents. Additional Details: {id.Result.Content}");
                }
                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);
  
                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));
                List<string> response = new List<string>();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    if (item.ItemID > 0)
                    {
                        response.Add(item.ItemID.ToString());
                    }
                }

                if (!string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    response.Add($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string?> DeleteContent(int? contentID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/item/{contentID}?comments={comments}");
                var id = client.ExecuteAsync(request, Method.Delete);
                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete content for content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID));

                StringBuilder response = new StringBuilder();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    response.Append(seperator);
                    if (item.ItemID > 0)
                    {
                        response.Append(item.ItemID);
                    }
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
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<ContentList?> GetContentItems(string? referenceName,
           string? filter = null,
           string? fields = null,
           string? sortDirection = null,
           string? sortField = null,
           int? take = 50,
           int? skip = 0)
        {
            try
            {
                var request = new RestRequest($"/{_options.locale}/list/{referenceName}?filter={filter}&fields={fields}&sortDirection={sortDirection}&sortField={sortField}&take={take}&skip={skip}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the content details for reference name: {referenceName}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var contentList = JsonSerializer.Deserialize<ContentList>(response.Result.Content, options);

                return contentList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
