namespace agility.models
{
    /// <summary>
    /// Represents a single page history record.
    /// </summary>
    public class PageHistory
    {
        public int PageItemID { get; set; }
        public int ContentItemID { get; set; }
        public int StateID { get; set; }
        public string? State { get; set; }
        public string? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public int CreatedByUserID { get; set; }
        public string? CreatedByUserEmail { get; set; }
        public int CreatedByServerUserID { get; set; }
        public string? ReleaseDate { get; set; }
        public string? PullDate { get; set; }
        public int CommentID { get; set; }
        public string? Comment { get; set; }
        public bool Pinned { get; set; }
        public int VersionNumber { get; set; }
    }

    /// <summary>
    /// Response containing page history records.
    /// </summary>
    public class PageHistoryResponse
    {
        public List<PageHistory>? Items { get; set; }
        public int TotalCount { get; set; }
    }
}
