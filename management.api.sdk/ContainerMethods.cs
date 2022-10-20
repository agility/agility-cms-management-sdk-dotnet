using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class ContainerMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        private Options _options = null;
        public ContainerMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);
        }
        public async Task<Container?> GetContainerById(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the container for id: {id}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var container = JsonSerializer.Deserialize<Container>(response.Result.Content, options);

                return container;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Container?> GetContainerByReferenceName(string? referenceName)
        {
            try
            {
                var request = new RestRequest($"/container/{referenceName}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the container for reference name: {referenceName}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var container = JsonSerializer.Deserialize<Container>(response.Result.Content, options);

                return container;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Container?> GetContainerSecurity(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}/security");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the container for id: {id}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var container = JsonSerializer.Deserialize<Container>(response.Result.Content, options);

                return container;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Container?>> GetContainerList()
        {
            try
            {
                var request = new RestRequest($"/container/list");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the containers. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var containers = JsonSerializer.Deserialize<List<Container>>(response.Result.Content, options);

                return containers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Notification?>> GetNotificationList(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}/notifications");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the containers notifications for id {id}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var notifications = JsonSerializer.Deserialize<List<Notification>>(response.Result.Content, options);

                return notifications;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Container?> SaveContainer(Container container)
        {
            try
            {
                var request = new RestRequest($"/container");
                request.AddJsonBody(container, "application/json");
                var response = client.ExecuteAsync(request, Method.Post);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable retreive the containers. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var containers = JsonSerializer.Deserialize<Container>(response.Result.Content, options);

                return containers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string?> DeleteContainer(int? id)
        {
            try
            {
                var request = new RestRequest($"/container/{id}");
                var response = client.ExecuteAsync(request, Method.Delete);
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete the container for containerID: {id}. Additional Details: {response.Result.Content}");
                }
                return response.Result.Content;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
