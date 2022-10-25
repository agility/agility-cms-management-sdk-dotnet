using agility.models;
using Microsoft.Extensions.Options;
using System;

namespace agility.utils
{
    public class AuthUtil
    {
        private readonly AppSettings _appSettings;
        private readonly AuthMethods _authMethods;
        private readonly agility.models.Options _options;
        public AuthUtil()
        {
            _options = new agility.models.Options();
            _appSettings = new AppSettings();
            _options.guid = Environment.GetEnvironmentVariable("Guid");
            _options.locale = Environment.GetEnvironmentVariable("Locale");
            IOptions<AppSettings> appSettingsOptions = Microsoft.Extensions.Options.Options.Create<AppSettings>(_appSettings);
            _authMethods = new AuthMethods(_options, appSettingsOptions);
        }
        public agility.models.Options GetTokenResponseData()
        {
            if (Environment.GetEnvironmentVariable("GenerateToken") != null && Environment.GetEnvironmentVariable("GenerateToken") == "True")
            {
                _options.refresh_token = Environment.GetEnvironmentVariable("RefreshToken");
                Environment.SetEnvironmentVariable("GenerateToken", "False");
            }
            else
            {
                var tokenResponseData = _authMethods.GetCurrentToken(_options.guid);
                if (tokenResponseData != null)
                {
                    _options.refresh_token = tokenResponseData.refresh_token;
                }
                else
                {
                    _options.refresh_token = Environment.GetEnvironmentVariable("RefreshToken");
                }
            }
            var _tokenResponseData = _authMethods.GetAuthorizationToken(_options.guid);
            _options.token = _tokenResponseData.access_token;
            _options.refresh_token = _tokenResponseData.refresh_token;
            return _options;
        }
    }
}
