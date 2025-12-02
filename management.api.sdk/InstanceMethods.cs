using agility.models;
using System.Text.Json;

namespace management.api.sdk
{
    /// <summary>
    /// Methods for instance-level operations.
    /// </summary>
    public class InstanceMethods
    {
        private Options _options;
        ExecuteMethods executeMethods;

        public InstanceMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Retrieves all locales for an instance.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <returns>A list of locale codes.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Locales?> GetLocales(string guid)
        {
            try
            {
                var apiPath = "/locales";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve locales. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var locales = JsonSerializer.Deserialize<Locales>(response.Result.Content, options);

                return locales;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
