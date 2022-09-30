using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class ClientInstance
    {
        public RestClient? CreateClient(string? baseUrl, string? guid)
        {
            var client = new RestClient($"{baseUrl}/{guid}");
                    
            client.AddDefaultHeader("Authorization", "Bearer YOUR TOKEN");
            client.AddDefaultHeader("Cache-Control", "no-cache");
            return client;
        }
      
    }
}