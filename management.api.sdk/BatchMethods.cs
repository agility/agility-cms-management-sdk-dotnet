﻿using agility.models;
using RestSharp;
using agility.enums;
using System.Text.Json;

namespace management.api.sdk
{
    public class BatchMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        private Options _options = null;
        public BatchMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);

        }

        /// <summary>
        /// Method to get the current status of the Batch.
        /// </summary>
        /// <param name="id">The batchID of the requested batch.</param>
        /// <returns>An object of Batch class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Batch?> GetBatch(int? id)
        {
            try
            {
                var request = new RestRequest($"/batch/{id}");
                var response = client.ExecuteAsync(request, Method.Get);

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
    }
}
