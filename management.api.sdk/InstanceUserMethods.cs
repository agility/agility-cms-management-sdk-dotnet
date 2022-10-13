using agility.models;
using RestSharp;

namespace management.api.sdk
{
    public class InstanceUserMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        private Options _options = null;
        public InstanceUserMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);
        }
        public async Task<string?> GetUsers(int? websiteID, bool includeSelfIfInternal = false)
        {
            try
            {
                var request = new RestRequest($"/user/list/{websiteID}/{includeSelfIfInternal}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> SaveUser(string? emailAddress, List<InstanceRole> roles, string? firstName = null, string? lastName = null)
        {
            try
            {
                var request = new RestRequest($"/user/save?emailAddress={emailAddress}&firstName={firstName}&lastName={lastName}");
                request.AddJsonBody(roles, "application/json");
                var response = client.ExecuteAsync(request, Method.Post).Result;
                return response.Content;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeleteUser(int? userID)
        {
            try
            {
                var request = new RestRequest($"/user/delete/{userID}");
                var response = client.ExecuteAsync(request, Method.Delete).Result;
                return response.Content;
            }
            catch
            {
                throw;
            }
        }
    }
}
