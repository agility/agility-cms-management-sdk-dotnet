using agility.models;
using System.Text.Json;

namespace management.api.sdk
{
    public class ModelMethods
    {
        private Options _options;
        ExecuteMethods executeMethods;
        public ModelMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Method to get the model by ID.
        /// </summary>
        /// <param name="id">The id of the requested model.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of Model class of the requested model.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Model?> GetContentModel(int? id, string guid)
        {
            try
            {
                var apiPath = $"/model/{id}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

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
        /// Method to get the model by referenceName.
        /// </summary>
        /// <param name="referenceName">The referenceName of the requested model.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of Model class of the requested model.</returns>
        /// <exception cref="ApplicationException"></exception>

        public async Task<Model?> GetModelByReferenceName(string? referenceName, string guid)
        {
            try
            {
                var apiPath = $"/model/{referenceName}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive model for referenceName: {referenceName}. Additional Details: {response.Result.Content}");
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
        /// <param name="guid">Current website guid.</param>
        /// <param name="includeModules">Boolean value to include modules.</param>
        /// <returns>A collection of Model object.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Model?>> GetContentModules(bool includeDefaults, string guid, bool includeModules = false)
        {
            try
            {
                var apiPath = $"/model/list/{includeDefaults}?includeModules={includeModules}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

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
        /// <param name="guid">Current website guid.</param>
        /// <returns>A collection of Model object.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Model?>> GetPageModules(string guid, bool includeDefault = false)
        {
            try
            {
 
                var apiPath = $"/model/list-page-modules/{includeDefault}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

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
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of Model class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Model?> SaveModel(Model model, string guid)
        {
            try
            {
                var apiPath = $"/model";
                var response = executeMethods.ExecutePost(apiPath, guid, model, _options.token);

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
        /// <param name="guid">Current website guid.</param>
        /// <returns>A string response if the model is deleted.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string?> DeleteModel(int? id, string guid)
        {
            try
            {
                var apiPath = $"/model/{id}";
                var response = executeMethods.ExecuteDelete(apiPath, guid, _options.token);
 
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
