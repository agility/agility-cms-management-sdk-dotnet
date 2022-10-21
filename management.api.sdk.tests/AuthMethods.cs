using agility.models;
using management.api.sdk;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using RestSharp;
using System;
using System.Linq;
using System.Text.Json;

namespace agility.utils
{
    public class AuthMethods
    {
        private agility.models.Options _options = null;
        ClientInstance _clientInstance = null;
        private readonly AppSettings _appSettings;
        public AuthMethods(agility.models.Options options, IOptions<AppSettings> settings)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            _appSettings = settings.Value;
        }

        public CloudTable GetTable()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_appSettings.storageConnectionString);
                var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
                var table = tableClient.GetTableReference(_appSettings.tableName);

                var create = table.CreateIfNotExists();
                return table;
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        public TokenResponseData GetAuthorizationToken(string? guid)
        {
            try
            {
                var baseURL = _options.DetermineBaseURL(guid);
                var client = new RestClient($"{baseURL}/oauth/refresh");
                client.AddDefaultHeader("Cache-Control", "no-cache");

                var request = new RestRequest($"{baseURL}/oauth/refresh");
                request.Method = Method.Post;
                request.AddParameter("refresh_token", _options.refresh_token, ParameterType.RequestBody);
                var response = client.ExecuteAsync(request);
                var resp = response.Result.Content;

                var tokenInfo = JsonSerializer.Deserialize<TokenResponseData>(resp);
                tokenInfo.RowKey = guid;
                var table = GetTable();

                table.Execute(TableOperation.InsertOrMerge(tokenInfo));
                return tokenInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }


        public TokenResponseData GetCurrentToken(string guid)
        {
            try
            {
                var acc = CloudStorageAccount.Parse(_appSettings.storageConnectionString);
                var tableClient = acc.CreateCloudTableClient();
                var table = tableClient.GetTableReference(_appSettings.tableName);
                if (!table.Exists())
                {
                    return null;
                }

                TableQuery<TokenResponseData> query = new TableQuery<TokenResponseData>().Where(
                    TableQuery.GenerateFilterCondition(
                        "RowKey",
                        QueryComparisons.Equal,
                        guid)
                    );
                var entity = table.ExecuteQuery(query).FirstOrDefault();
                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
          
        }
        
    }
}
