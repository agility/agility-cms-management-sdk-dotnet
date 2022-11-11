using Microsoft.Azure.Cosmos.Table;
using System;

namespace agility.models
{
    public class TokenResponseData : TableEntity
    {

        public TokenResponseData()
        {
            PartitionKey = "Token";
            RowKey = Guid.NewGuid().ToString();
        }
        public string? access_token { get; set; }
        public long? expires_in { get; set; }
        public string? token_type { get; set; }
        public string? refresh_token { get; set; }
    }
}
