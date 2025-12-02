namespace agility.models
{
    /// <summary>
    /// Represents a webhook configuration in Agility CMS.
    /// </summary>
    public class Webhook
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? InstanceGuid { get; set; }
        public bool Enabled { get; set; }
        public bool ContentWorkflowEvents { get; set; }
        public bool ContentPublishEvents { get; set; }
        public bool ContentSaveEvents { get; set; }
    }
}
