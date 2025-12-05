namespace agility.models
{
    /// <summary>
    /// Represents a website listing in the user's access list.
    /// </summary>
    public class WebsiteListing
    {
        public string? OrgCode { get; set; }
        public string? OrgName { get; set; }
        public string? WebsiteName { get; set; }
        public string? WebsiteNameStripped { get; set; }
        public string? DisplayName { get; set; }
        public string? Guid { get; set; }
        public int WebsiteID { get; set; }
        public bool IsCurrent { get; set; }
        public string? ManagerUrl { get; set; }
        public string? Version { get; set; }
        public bool IsOwner { get; set; }
        public bool IsDormant { get; set; }
        public bool IsRestoring { get; set; }
        public int? TeamID { get; set; }
    }
}
