namespace agility.models
{
    public class Options
    {
        public string? locale { get; set; }
        public string? token { get; set; }
        public string? baseUrl { get; set; }
        public string? guid { get; set; }

        public string? refresh_token { get; set; }

        public string DetermineBaseURL(string guid)
        {
            if (guid.EndsWith("-d"))
            {
                if (Environment.GetEnvironmentVariable("Local") != null)
                {
                    var runLocally = Environment.GetEnvironmentVariable("Local");
                    if(runLocally == "True")
                    {
                        baseUrl = "https://localhost:5050/api/v1/instance";
                    }
                    else
                    {
                        baseUrl = "https://mgmt-dev.aglty.io/api/v1/instance";
                    }
                }
                else
                {
                    baseUrl = "https://mgmt-dev.aglty.io/api/v1/instance";
                }
            }
            else if (guid.EndsWith("-u"))
            {
                baseUrl = "https://mgmt-us.aglty.io/api/v1/instance";
            }
            else if (guid.EndsWith("-ca"))
            {
                baseUrl = "https://mgmt-ca.aglty.io/api/v1/instance";
            }
            else if (guid.EndsWith("-eu"))
            {
                baseUrl = "https://mgmt-eu.aglty.io/api/v1/instance";
            }
            else if (guid.EndsWith("-aus"))
            {
                baseUrl = "https://mgmt-aus.aglty.io/api/v1/instance";
            }
            else
            {
                baseUrl = "https://mgmt-us.aglty.io/";
            }
            return baseUrl;
        }

        public string DetermineBaseForAuth(string guid)
        {
            if (guid.EndsWith("-d"))
            {
                if (Environment.GetEnvironmentVariable("Local") != null)
                {
                    var runLocally = Environment.GetEnvironmentVariable("Local");
                    if (runLocally == "True")
                    {
                        var baseUrl = Environment.GetEnvironmentVariable("BaseLocalURL");
                        return baseUrl;
                    }
                    else
                    {
                        return "https://mgmt-dev.aglty.io";
                    }
                }
                return "https://mgmt-dev.aglty.io";
            }
            else if (guid.EndsWith("-u"))
            {
                return "https://mgmt-us.aglty.io";
            }
            else if (guid.EndsWith("-ca"))
            {
                return "https://mgmt-ca.aglty.io";
            }
            else if (guid.EndsWith("-eu"))
            {
                return "https://mgmt-eu.aglty.io";
            }
            else if (guid.EndsWith("-aus"))
            {
                return "https://mgmt-aus.aglty.io";
            }
            else
            {
                return "https://mgmt-us.aglty.io/";
            }
        }
    }
}
