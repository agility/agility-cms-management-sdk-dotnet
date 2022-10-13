using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class ClientInstance
    {
        public RestClient? CreateClient(Options options)
        {
            var baseURL = options.DetermineBaseURL(options.guid);
            var client = new RestClient($"{baseURL}/{options.guid}");

            client.AddDefaultHeader("Authorization", $"Bearer {options.token}");
            client.AddDefaultHeader("Cache-Control", "no-cache");
            return client;
        }
    }
}