using agility.models;
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

        public async Task<string?> GetBatch(int? id)
        {
            try
            {
                var request = new RestRequest($"/batch/{id}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Batch> Retry(Func<Task<Batch>> method, int duration = 3000, int retryCount = 3)
        {
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
                        throw new Exception();
                    }
                }
                catch
                {
                    if (--retryCount == 0)
                        throw;
                    await Task.Delay(duration);
                }
            }
        }
    }
}
