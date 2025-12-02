using agility.models;
using agility.enums;
using System.Text.Json;

namespace management.api.sdk
{
    public class BatchMethods
    {
        private Options _options;
        ExecuteMethods executeMethods;
        public BatchMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Method to get the current status of the Batch.
        /// </summary>
        /// <param name="id">The batchID of the requested batch.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of Batch class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Batch?> GetBatch(int? id, string guid)
        {
            try
            {
                //var request = new RestRequest($"/batch/{id}");
                var apiPath = $"/batch/{id}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
                //var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive the batch details for batchID {id}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var batch = JsonSerializer.Deserialize<Batch>(response.Result.Content, options);
                return batch;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to retry a batch processing.
        /// </summary>
        /// <param name="method">A method of type Batch class.</param>
        /// <returns>Returns a completed batch object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Batch> Retry(Func<Task<Batch>> method)
        {
            var retryCount = _options.retryCount;
            var duration = _options.duration;
            if (retryCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(retryCount));

            while (true)
            {
                try
                {
                    var batch = await method();
                    if(batch.BatchState == BatchState.Processed)
                    {
                        return batch;
                    }
                    else
                    {
                        --retryCount;
                        if (--retryCount <= 0)
                        {
                            throw new ApplicationException("Timeout exceeded but operation still in progress. Please check the Batches page in the Agility Content Manager app.");
                        }
                        await Task.Delay(duration);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Publishes a batch.
        /// </summary>
        /// <param name="batchID">The batch ID to publish.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>The batch ID of the publish operation.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int> PublishBatch(int batchID, string guid, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/batch/{batchID}/publish";
                var response = executeMethods.ExecutePost(apiPath, guid, null, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to publish the batch with id: {batchID}. Additional Details: {response.Result.Content}");
                }

                var resultBatchID = JsonSerializer.Deserialize<int>(response.Result.Content);

                if (returnBatchId)
                {
                    return resultBatchID;
                }

                await Retry(async () => await GetBatch(resultBatchID, guid));
                return resultBatchID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Unpublishes a batch.
        /// </summary>
        /// <param name="batchID">The batch ID to unpublish.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>The batch ID of the unpublish operation.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int> UnpublishBatch(int batchID, string guid, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/batch/{batchID}/unpublish";
                var response = executeMethods.ExecutePost(apiPath, guid, null, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to unpublish the batch with id: {batchID}. Additional Details: {response.Result.Content}");
                }

                var resultBatchID = JsonSerializer.Deserialize<int>(response.Result.Content);

                if (returnBatchId)
                {
                    return resultBatchID;
                }

                await Retry(async () => await GetBatch(resultBatchID, guid));
                return resultBatchID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Approves a batch.
        /// </summary>
        /// <param name="batchID">The batch ID to approve.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>The batch ID of the approve operation.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int> ApproveBatch(int batchID, string guid, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/batch/{batchID}/approve";
                var response = executeMethods.ExecutePost(apiPath, guid, null, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to approve the batch with id: {batchID}. Additional Details: {response.Result.Content}");
                }

                var resultBatchID = JsonSerializer.Deserialize<int>(response.Result.Content);

                if (returnBatchId)
                {
                    return resultBatchID;
                }

                await Retry(async () => await GetBatch(resultBatchID, guid));
                return resultBatchID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Declines a batch.
        /// </summary>
        /// <param name="batchID">The batch ID to decline.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>The batch ID of the decline operation.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int> DeclineBatch(int batchID, string guid, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/batch/{batchID}/decline";
                var response = executeMethods.ExecutePost(apiPath, guid, null, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to decline the batch with id: {batchID}. Additional Details: {response.Result.Content}");
                }

                var resultBatchID = JsonSerializer.Deserialize<int>(response.Result.Content);

                if (returnBatchId)
                {
                    return resultBatchID;
                }

                await Retry(async () => await GetBatch(resultBatchID, guid));
                return resultBatchID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Requests approval for a batch.
        /// </summary>
        /// <param name="batchID">The batch ID to request approval for.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="returnBatchId">If true, returns batch ID immediately without waiting for completion.</param>
        /// <returns>The batch ID of the request approval operation.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<int> RequestApprovalBatch(int batchID, string guid, bool returnBatchId = false)
        {
            try
            {
                var apiPath = $"/batch/{batchID}/requestapproval";
                var response = executeMethods.ExecutePost(apiPath, guid, null, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to request approval for the batch with id: {batchID}. Additional Details: {response.Result.Content}");
                }

                var resultBatchID = JsonSerializer.Deserialize<int>(response.Result.Content);

                if (returnBatchId)
                {
                    return resultBatchID;
                }

                await Retry(async () => await GetBatch(resultBatchID, guid));
                return resultBatchID;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Retrieves all batch-related enum types for developer discovery.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <returns>BatchTypesResponse containing all batch enum types.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<BatchTypesResponse?> GetBatchTypes(string guid)
        {
            try
            {
                var apiPath = "/batch/types";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve batch types. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var batchTypes = JsonSerializer.Deserialize<BatchTypesResponse>(response.Result.Content, options);

                return batchTypes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
