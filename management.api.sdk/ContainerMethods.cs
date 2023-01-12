﻿using agility.models;
using System.Text.Json;

namespace management.api.sdk
{
    public class ContainerMethods
    {
        private Options _options;
        ExecuteMethods executeMethods;
        public ContainerMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Method to Get Container by ID.
        /// </summary>
        /// <param name="id">The container id of the requested container.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of the Container class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Container?> GetContainerById(int? id, string guid)
        {
            try
            {
                var apiPath = $"/container/{id}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive the container for id: {id}. Additional Details: {response.Result.Content}");
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

        /// <summary>
        /// Method to Get Container by Reference Name.
        /// </summary>
        /// <param name="referenceName">The container referenceName of the requested container.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of the Container class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Container?> GetContainerByReferenceName(string? referenceName, string guid)
        {
            try
            {
                var apiPath = $"/container/{referenceName}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive the container for reference name: {referenceName}. Additional Details: {response.Result.Content}");
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

        /// <summary>
        /// Method to Get Container Security by container id.
        /// </summary>
        /// <param name="id">The container id of the requested container.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of the Container class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Container?> GetContainerSecurity(int? id, string guid)
        {
            try
            {
                var apiPath = $"/container/{id}/security";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    if(response.Result.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return new Container();
                    }
                    throw new ApplicationException($"Unable to retreive the container for id: {id}. Additional Details: {response.Result.Content}");
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

        /// <summary>
        /// Method to Get All Container List for the website.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object collection of the Container class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Container?>> GetContainerList(string guid)
        {
            try
            {
                var apiPath = $"/container/list";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive the containers. Additional Details: {response.Result.Content}");
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

        /// <summary>
        /// Method to Get Container Notifications by ID.
        /// </summary>
        /// <param name="id">The container id of the requested container.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object collection of the Notification class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Notification?>> GetNotificationList(int? id, string guid)
        {
            try
            {
                var apiPath = $"/container/{id}/notifications";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive the containers notifications for id {id}. Additional Details: {response.Result.Content}");
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

        /// <summary>
        /// Method to save/update a container.
        /// </summary>
        /// <param name="container">An Container type object to create or update a container</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of the Container class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Container?> SaveContainer(Container container, string guid)
        {
            try
            {
                var apiPath = $"/container";
                var response = executeMethods.ExecutePost(apiPath, guid, container, _options.token);
 
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive the containers. Additional Details: {response.Result.Content}");
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

        /// <summary>
        /// Method to Delete a Container by ID.
        /// </summary>
        /// <param name="id">The container id of the requested container.</param>
        /// <param name="guid">Current website guid.</param>
        /// <returns>Returns a string response if the container has been deleted.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string?> DeleteContainer(int? id, string guid)
        {
            try
            {
                var apiPath = $"/container/{id}";
                var response = executeMethods.ExecuteDelete(apiPath, guid, _options.token);
 
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
