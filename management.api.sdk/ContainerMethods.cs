using agility.models;
using RestSharp;

namespace management.api.sdk
{
    public class ContainerMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        public ContainerMethods(string? baseAddress, string? guid)
        {
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(baseAddress, guid);
        }
        public async Task<string?> GetContainerById(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetContainerByReferenceName(string? referenceName)
        {
            try
            {
                var request = new RestRequest($"/container/{referenceName}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetContainerSecurity(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}/security");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetContainerList()
        {
            try
            {
                var request = new RestRequest($"/container/list");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetNotificationList(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}/notifications");
                var response = client.ExecuteAsync(request, Method.Get).Result;
                return response.Content;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> SaveContainer(Container container)
        {
            try
            {
                var request = new RestRequest($"/container");
                request.AddJsonBody(container, "application/json");
                var response = client.ExecuteAsync(request, Method.Post).Result;
                return response.Content;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeleteContainer(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}");
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
