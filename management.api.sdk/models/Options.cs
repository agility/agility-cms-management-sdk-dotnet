namespace agility.models
{
    public class Options
    {
        public string? locale { get; set; }
        public string? token { get; set; }
        public string? baseUrl { get; set; }
        public string? guid { get; set; }

        public string? refresh_token { get; set; }

        public int duration { get; set; } = 3000;

        public int retryCount { get; set; } = 500;


        public string DetermineBaseURL(string guid)
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
                return "https://mgmt.aglty.io";
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
                return "https://mgmt.aglty.io";
            }
        }
    }
}
