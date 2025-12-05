namespace agility.models
{
    /// <summary>
    /// Represents a single comment on an item.
    /// </summary>
    public class ItemComment
    {
        public string? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public int CreatedByUserID { get; set; }
        public int CreatedByServerUserID { get; set; }
        public int CommentID { get; set; }
        public string? CommentText { get; set; }
        public bool Pinned { get; set; }
        public string? EmailAddress { get; set; }
    }

    /// <summary>
    /// Response containing item comments.
    /// </summary>
    public class ItemComments
    {
        public List<ItemComment>? Items { get; set; }
        public int TotalCount { get; set; }
    }
}
