using agility.models;
using System.Text.Json;

namespace management.api.sdk
{
    /// <summary>
    /// Methods for server user operations.
    /// </summary>
    public class ServerUserMethods
    {
        private Options _options;
        ExecuteMethods executeMethods;

        public ServerUserMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Gets the current server user information.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <returns>The current server user.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<ServerUser?> Me(string guid)
        {
            try
            {
                var apiPath = "/users/me";
                var response = executeMethods.ExecuteServerGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve user information. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var user = JsonSerializer.Deserialize<ServerUser>(response.Result.Content, options);

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets specific server user information by ID.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="serverUserID">The server user ID.</param>
        /// <returns>The requested server user.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<object?> You(string guid, int serverUserID)
        {
            try
            {
                var apiPath = $"/users/you?srvUserID={serverUserID}";
                var response = executeMethods.ExecuteServerGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve user information. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var user = JsonSerializer.Deserialize<object>(response.Result.Content, options);

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
