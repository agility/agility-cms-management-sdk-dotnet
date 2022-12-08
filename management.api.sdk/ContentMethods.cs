using agility.models;
using System.Text;
using System.Text.Json;

namespace management.api.sdk
{
    public class ContentMethods
    {
        BatchMethods _batchMethods = null;
        private Options _options = null;
        ExecuteMethods executeMethods = null;
        public ContentMethods(Options options)
        {
            _options = options;
            _batchMethods = new BatchMethods(options);
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Method to Get a Content item by Locale and ContentID
        /// </summary>
        /// <param name="contentID">The contentid of the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <returns>An object of ContentItem class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<ContentItem> GetContentItem(int? contentID, string guid, string locale)
        {
            try
            {
                var apiPath = $"/{locale}/item/{contentID}";

                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

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

        /// <summary>
        /// Method to publish a content in a batch for a contentID.
        /// </summary>
        /// <param name="contentID">The contentid of the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a contentID of the requested content.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> PublishContent(int? contentID, string guid, string locale, string? comments = null)
        {
            try
            {
                var apiPath = $"/{locale}/item/{contentID}/publish?comments={comments}";

                var id = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to publish the content for id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int?>(id.Result.Content);
  
                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdContent = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdContent = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to publish the batch for contentID: {contentID}");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to publish the batch for contentID: {contentID}");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdContent;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Method to unpublish a content in a batch for a contentID.
        /// </summary>
        /// <param name="contentID">The contentid of the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a contentID of the requested content.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> UnPublishContent(int? contentID, string guid, string locale, string? comments = null)
        {
            try
            {
                var apiPath = $"/{locale}/item/{contentID}/unpublish?comments={comments}";

                var id = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to un-publish the content for id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdContent = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdContent = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to un-publish the batch for contentID: {contentID}");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to un-publish the batch for contentID: {contentID}");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdContent;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Method to request approval for a content in a batch for a contentID.
        /// </summary>
        /// <param name="contentID">The contentid of the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string contentID of the requested content.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> ContentRequestApproval(int? contentID, string guid, string locale, string? comments = null)
        {
            try
            {
                var apiPath = $"/{locale}/item/{contentID}/request-approval?comments={comments}";

                var id = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to request for approval for the content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdContent = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdContent = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to process the request approval of batch for contentID: {contentID}");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to process the request approval of batch for contentID: {contentID}");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdContent;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Method to approve a content in a batch for a contentID.
        /// </summary>
        /// <param name="contentID">The contentid of the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string contentID of the requested content.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> ApproveContent(int? contentID, string guid, string locale, string? comments = null)
        {
            try
            {
                var apiPath = $"/{locale}/item/{contentID}/approve?comments={comments}";

                var id = executeMethods.ExecuteGet(apiPath, guid, _options.token);
                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to approve content for the content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdContent = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdContent = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to process the approve content of the batch for contentID: {contentID}");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to process the approve content of the batch for contentID: {contentID}");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdContent;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to decline a content in a batch for a contentID.
        /// </summary>
        /// <param name="contentID">The contentid of the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string contentID of the requested content.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> DeclineContent(int? contentID, string guid, string locale, string? comments = null)
        {
            try
            {
                var apiPath = $"/{locale}/item/{contentID}/decline?comments={comments}";

                var id = executeMethods.ExecuteGet(apiPath, guid, _options.token);
                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to decline content for the content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdContent = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdContent = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to decline content for the batch for contentID: {contentID}");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to decline content for the batch for contentID: {contentID}");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdContent;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Method to save a content in a batch for a contentItem object.
        /// </summary>
        /// <param name="contentItem">A contentItem object to create or update a content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <returns>Returns a string contentID of the requested content.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int> SaveContentItem(ContentItem? contentItem, string guid, string locale)
        {
            try
            {
                var apiPath = $"/{locale}/item";
                var id = executeMethods.ExecutePost(apiPath, guid, contentItem, _options.token);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save content. Additional Details: {id.Result.Content}");
                }
                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);
                
                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdContent = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdContent = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to create content.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to create content.");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdContent;
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
        /// Method to save a collection of contents in a batch for a List of contentItem object.
        /// </summary>
        /// <param name="contentItems">A collection of contentItems object to create or update multiple contents.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <returns>A list of string which consists of the processed contentID's for the batch request.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<object?>> SaveContentItems(List<ContentItem?> contentItems, string guid, string locale)
        {
            try
            {
                var apiPath = $"/{locale}/item/multi";
                var id = executeMethods.ExecutePost(apiPath, guid, contentItems, _options.token);

                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save contents. Additional Details: {id.Result.Content}");
                }
                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);
  
                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));
                List<object> response = new List<object>();
                string seperator = string.Empty;
                foreach (var item in batch.Items)
                {
                    if (item.ItemID > 0)
                    {
                        response.Add(item.ItemID);
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

        /// <summary>
        /// Method to delete a content in a batch for a contentID.
        /// </summary>
        /// <param name="contentID">The contentid of the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="comments">Additional comments for a batch request.</param>
        /// <returns>Returns a string contentID of the requested content.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int?> DeleteContent(int? contentID, string guid, string locale, string? comments = null)
        {
            try
            {
                var apiPath = $"/{locale}/item/{contentID}?comments={comments}";

                var id = executeMethods.ExecuteDelete(apiPath, guid, _options.token);
                if (id.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete content for content id: {contentID}. Additional Details: {id.Result.Content}");
                }

                var batchID = JsonSerializer.Deserialize<int>(id.Result.Content);

                var batch = await _batchMethods.Retry(async () => await GetBatchObject(batchID, guid));

                int createdContent = 0;

                if (string.IsNullOrWhiteSpace(batch.ErrorData))
                {
                    if (batch.Items[0] != null)
                    {
                        if (batch.Items[0].ItemID > 0)
                        {
                            createdContent = batch.Items[0].ItemID;
                        }
                        else
                        {
                            throw new ApplicationException($"Unable to delete content for the batch for contentID: {contentID}");
                        }
                    }
                    else
                    {
                        throw new ApplicationException($"Unable to delete content for the batch for contentID: {contentID}");
                    }

                }
                else
                {
                    throw new ApplicationException($"Error(s) found while processing the batch. Additional details on error {batch.ErrorData}");
                }

                return createdContent;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Method to list the content items for a container.
        /// </summary>
        /// <param name="referenceName">The referenceName of the container for the requested content.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="locale">Current website locale.</param>
        /// <param name="filter">The filter condition for the requested content.</param>
        /// <param name="fields">The fields mapped to the container.</param>
        /// <param name="sortDirection">The direction to sort the result.</param>
        /// <param name="sortField">The field on which the sort needs to be performed.</param>
        /// <param name="take">The page size for the result.</param>
        /// <param name="skip">The record offset for the result.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<ContentList?> GetContentItems(string? referenceName, 
           string guid, 
           string locale,
           string? filter = null,
           string? fields = null,
           string? sortDirection = null,
           string? sortField = null,
           int? take = 50,
           int? skip = 0)
        {
            try
            {
                var apiPath = $"/{locale}/list/{referenceName}?filter={filter}&fields={fields}&sortDirection={sortDirection}&sortField={sortField}&take={take}&skip={skip}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

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
