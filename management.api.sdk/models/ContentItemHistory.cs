namespace agility.models
{
    /// <summary>
    /// Response containing content item history records.
    /// </summary>
    public class ContentItemHistoryResponse
    {
        public List<ContentItemHistory>? Items { get; set; }
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Represents a single content item history record.
    /// </summary>
    public class ContentItemHistory
    {
        public int ContentItemID { get; set; }
        public int StateID { get; set; }
        public string? State { get; set; }
        public string? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public int CreatedByUserID { get; set; }
        public int CreatedByServerUserID { get; set; }
        public string? CreatedByUserEmail { get; set; }
        public string? ReleaseDate { get; set; }
        public string? PullDate { get; set; }
        public int CommentID { get; set; }
        public string? Comment { get; set; }
        public bool Pinned { get; set; }
        public int VersionNumber { get; set; }
    }
}
