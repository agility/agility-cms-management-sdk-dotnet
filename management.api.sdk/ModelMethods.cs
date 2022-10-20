using RestSharp;
using agility.models;
using System.Text.Json;

namespace management.api.sdk
{
    public class ModelMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        private Options _options = null;
        public ModelMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);
        }
        public async Task<Model?> GetContentModel(int? id)
        {
            try
            {
                var request = new RestRequest($"/model/{id}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive model for id: {id}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var model = JsonSerializer.Deserialize<Model>(response.Result.Content, options);

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Model?>> GetContentModules(bool includeDefaults, bool includeModules = false)
        {
            try
            {
                var request = new RestRequest($"/model/list/{includeDefaults}?includeModules={includeModules}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive content modules. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var models = JsonSerializer.Deserialize<List<Model>>(response.Result.Content, options);

                return models;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<Model?>> GetPageModules(bool includeDefault = false)
        {
            try
            {
                var request = new RestRequest($"/model/list-page-modules/{includeDefault}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive page modules. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var models = JsonSerializer.Deserialize<List<Model>>(response.Result.Content, options);

                return models;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Model?> SaveModel(Model model)
        {
            try
            {
                var request = new RestRequest($"/model");
                request.AddJsonBody(model, "application/json");

                var response = client.ExecuteAsync(request, Method.Post);
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save model. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var retModel = JsonSerializer.Deserialize<Model>(response.Result.Content, options);

                return retModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string?> DeleteModel(int? id)
        {
            try
            {
                var request = new RestRequest($"/model/{id}");
                var response = client.ExecuteAsync(request, Method.Delete);
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete model for id {id}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var status = JsonSerializer.Deserialize<string>(response.Result.Content, options);

                return status;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
