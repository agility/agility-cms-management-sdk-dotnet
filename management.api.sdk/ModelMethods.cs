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

        /// <summary>
        /// Method to get the model by ID.
        /// </summary>
        /// <param name="id">The id of the requested model.</param>
        /// <returns>An object of Model class of the requested model.</returns>
        /// <exception cref="ApplicationException"></exception>
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

        /// <summary>
        /// Method to list the content models.
        /// </summary>
        /// <param name="includeDefaults">Boolean value to include defaults.</param>
        /// <param name="includeModules">Boolean value to include modules.</param>
        /// <returns>A collection of Model object.</returns>
        /// <exception cref="ApplicationException"></exception>
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

        /// <summary>
        /// Method to list the page models.
        /// </summary>
        /// <param name="includeDefault">Boolean value to include defaults.</param>
        /// <returns>A collection of Model object.</returns>
        /// <exception cref="ApplicationException"></exception>
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

        /// <summary>
        /// Method to save a ContentModel.
        /// </summary>
        /// <param name="model">The object of Model to for the requested model.</param>
        /// <returns>An object of Model class.</returns>
        /// <exception cref="ApplicationException"></exception>
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

        /// <summary>
        /// Method to delete the Model.
        /// </summary>
        /// <param name="id">The id for the requested model.</param>
        /// <returns>A string response if the model is deleted.</returns>
        /// <exception cref="ApplicationException"></exception>
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
