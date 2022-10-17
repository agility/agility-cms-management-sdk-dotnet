using agility.models;
using RestSharp;
using System.Dynamic;
using System.Text;
using System.Text.Json;

namespace management.api.sdk
{
    public class AuthMethods
    {
        private Options _options = null;
        ClientInstance _clientInstance = null;
        public AuthMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
        }

        public async Task<string> GetAuthorizationToken(string? guid, string? path)
        {

            var baseURL = _options.DetermineBaseForAuth(guid);
            var client = new RestClient($"{baseURL}/oauth/refresh");
            client.AddDefaultHeader("Cache-Control", "no-cache");

            var request = new RestRequest($"{baseURL}/oauth/refresh");
            request.Method = Method.Post;
            request.AddParameter("refresh_token", _options.refresh_token, ParameterType.RequestBody);
            var response = client.ExecuteAsync(request);
            var resp = response.Result.Content;
            ExportToken(resp, path);
            return resp;
        }

        private void ExportToken(string? content, string? path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
