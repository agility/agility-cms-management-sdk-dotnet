using agility.models;
using System;

namespace agility.utils
{
    public class AuthUtil
    {
        private readonly agility.models.Options _options;

        public AuthUtil()
        {
            // Load environment variables from .env file if it exists
            EnvLoader.Load();

            _options = new agility.models.Options();
        }

        public agility.models.Options GetTokenResponseData()
        {
            // Get the access token from environment variable
            string? accessToken = Environment.GetEnvironmentVariable("AccessToken");
            
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException(
                    "AccessToken environment variable is not set. " +
                    "Please create a .env file based on .env.example and populate it with your PAT.");
            }

            _options.token = accessToken;
            return _options;
        }
    }
}
