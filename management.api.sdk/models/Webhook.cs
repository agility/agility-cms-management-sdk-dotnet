namespace agility.models
{
    /// <summary>
    /// Represents a webhook configuration in Agility CMS.
    /// </summary>
    public class Webhook
    {
        public string? webhookID { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
        public string[]? events { get; set; }
        public bool isActive { get; set; }
        public string? instanceGuid { get; set; }
    }
}
