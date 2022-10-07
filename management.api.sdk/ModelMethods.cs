using RestSharp;
using agility.models;

namespace management.api.sdk
{
    public class ModelMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        public ModelMethods(string? guid)
        {
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(guid);
        }
        public async Task<string?> GetContentModel(int? id)
        {
            try
            {
                var request = new RestRequest($"/model/{id}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetContentModules(bool includeDefaults, bool includeModules = false)
        {
            try
            {
                var request = new RestRequest($"/model/list/{includeDefaults}?includeModules={includeModules}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetPageModules(bool includeDefault = false)
        {
            try
            {
                var request = new RestRequest($"/model/list-page-modules/{includeDefault}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> SaveModel(Model model)
        {
            try
            {
                var request = new RestRequest($"/model");
                request.AddJsonBody(model, "application/json");
                var response = client.ExecuteAsync(request, Method.Post).Result;
                return response.Content;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeleteModel(int? id)
        {
            try
            {
                var request = new RestRequest($"/model/{id}");
                var response = client.ExecuteAsync(request, Method.Delete).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }
    }
}
