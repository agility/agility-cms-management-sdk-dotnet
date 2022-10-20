﻿using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class InstanceUserMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        private Options _options = null;
        public InstanceUserMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);
        }
        public async Task<List<WebsiteUser?>> GetUsers(int? websiteID, bool includeSelfIfInternal = false)
        {
            try
            {
                var request = new RestRequest($"/user/list/{websiteID}/{includeSelfIfInternal}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retreive users for website: {websiteID}. Additional Details: {response.Result.Content}");
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

        public async Task<InstanceUser?> SaveUser(string? emailAddress, List<InstanceRole> roles, string? firstName = null, string? lastName = null)
        {
            try
            {
                var request = new RestRequest($"/user/save?emailAddress={emailAddress}&firstName={firstName}&lastName={lastName}");
                request.AddJsonBody(roles, "application/json");
                var response = client.ExecuteAsync(request, Method.Post);

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

        public async Task<string?> DeleteUser(int? userID)
        {
            try
            {
                var request = new RestRequest($"/user/delete/{userID}");
                var response = client.ExecuteAsync(request, Method.Delete);
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
