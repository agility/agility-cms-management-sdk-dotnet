using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class ClientInstance
    {
        private string BaseUrl { get; set; } = "https://api.aglty.io";
        public RestClient? CreateClient(string? guid)
        {
            var baseURL = DetermineBaseURL(guid);
            var client = new RestClient($"{baseURL}/{guid}");

            client.AddDefaultHeader("Authorization", "Bearer <<TOKEN>>");
            client.AddDefaultHeader("Cache-Control", "no-cache");
            return client;
        }

        private string DetermineBaseURL(string guid)
        {
            if (guid.EndsWith("-d"))
            {
                BaseUrl = "https://mgmt-dev.aglty.io/api/v1/instance";
            }
            else if (guid.EndsWith("-u"))
            {
                BaseUrl = "https://api.aglty.io";
            }
            else if (guid.EndsWith("-ca"))
            {
                BaseUrl = "https://api-ca.aglty.io";
            }
            else if (guid.EndsWith("-eu"))
            {
                BaseUrl = "https://api-eu.aglty.io";
            }
            else
            {
                BaseUrl = "https://api.aglty.io";
            }
            return BaseUrl;
        }

    }
}