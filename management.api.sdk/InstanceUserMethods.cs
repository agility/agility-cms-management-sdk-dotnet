using agility.models;
using System.Text.Json;

namespace management.api.sdk
{
    public class InstanceUserMethods
    {
        private Options _options = null;
        ExecuteMethods executeMethods = null;
        public InstanceUserMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Method to List Users for a website.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <returns>A collection of Website User.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<WebsiteUser?>> GetUsers(string guid)
        {
            try
            {
                var apiPath = $"/user/list";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive users. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var users = JsonSerializer.Deserialize<List<WebsiteUser>>(response.Result.Content, options);

                return users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Method to Create a User.
        /// </summary>
        /// <param name="emailAddress">The email address of the requested user.</param>
        /// <param name="roles">Object of InstanceRole class for the requested user.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="firstName">The first name of the requested user.</param>
        /// <param name="lastName">The last name of the requested user.</param>
        /// <returns>An object of the InstanceUser class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<InstanceUser?> SaveUser(string? emailAddress, List<InstanceRole> roles, string guid, string? firstName = null, string? lastName = null)
        {
            try
            {
                var apiPath = $"/user/save?emailAddress={emailAddress}&firstName={firstName}&lastName={lastName}";
                var response = executeMethods.ExecutePost(apiPath, guid, roles, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to create user for emailAddress: {emailAddress}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var user = JsonSerializer.Deserialize<InstanceUser>(response.Result.Content, options);

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Method to delete user.
        /// </summary>
        /// <param name="userID">The userID of the requested user.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>A string response if a user is deleted.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string?> DeleteUser(int? userID, string guid)
        {
            try
            {
                var apiPath = $"/user/delete/{userID}";
                var response = executeMethods.ExecuteDelete(apiPath, guid, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete the user for userID: {userID}. Additional Details: {response.Result.Content}");
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
