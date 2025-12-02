namespace agility.models
{
    /// <summary>
    /// Represents a server user in the Agility CMS system.
    /// </summary>
    public class ServerUser
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? EmailAddress { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsInternalUser { get; set; }
        public bool IsSuspended { get; set; }
        public bool IsProfileComplete { get; set; }
        public bool AdminAccess { get; set; }
        public string? CurrentWebsite { get; set; }
        public int UserTypeID { get; set; }
        public string? TimeZoneRegion { get; set; }
        public string? Password { get; set; }
        public string? PasswordQuestion { get; set; }
        public string? PasswordAnswer { get; set; }
        public List<WebsiteListing>? WebsiteAccess { get; set; }
        public string? JobRole { get; set; }
        public string? CreatedDate { get; set; }
    }
}
