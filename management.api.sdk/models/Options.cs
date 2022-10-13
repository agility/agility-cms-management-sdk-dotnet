namespace agility.models
{
    public class Options
    {
        public string? locale { get; set; }
        public string? token { get; set; }
        public string? baseUrl { get; set; }
        public string? guid { get; set; }

        public string DetermineBaseURL(string guid)
        {
            if (guid.EndsWith("-d"))
            {
                baseUrl = "https://mgmt-dev.aglty.io/api/v1/instance";
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
                baseUrl = "https://mgmt-eu.aglty.io/api/v1/instanceo";
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
    }
}
