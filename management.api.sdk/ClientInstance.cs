using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class ClientInstance
    {
        /// <summary>
        /// Method to create a RestClient for the SDK.
        /// </summary>
        /// <param name="options">An object of type Options.</param>
        /// <returns></returns>
        public RestClient? CreateClient(Options options)
        {
            var baseURL = options.DetermineBaseURL(options.guid);
            var client = new RestClient($"{baseURL}/api/v1/instance/{options.guid}");

            client.AddDefaultHeader("Authorization", $"Bearer {options.token}");
            client.AddDefaultHeader("Cache-Control", "no-cache");
            return client;
        }
    }
}