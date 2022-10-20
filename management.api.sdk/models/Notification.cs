namespace agility.models
{
    public class Notification
    {
        public int? NotificationID { get; set; }
        public int? UserID { get; set; }
        public int? ContentViewID { get; set; }
        public int? PageItemContainerID { get; set; }
        public int? CreatedAuthorID { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? NotificationTypeName { get; set; }
    }
}
