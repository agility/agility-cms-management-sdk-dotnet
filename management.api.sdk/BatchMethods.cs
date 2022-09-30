using agility.models;
using RestSharp;

namespace management.api.sdk
{
    public class BatchMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;

        public BatchMethods(string? baseAddress, string? guid)
        {
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(baseAddress, guid);

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
    }
}
