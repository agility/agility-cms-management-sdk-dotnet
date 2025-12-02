namespace agility.models
{
    /// <summary>
    /// Parameters for listing and filtering content items.
    /// </summary>
    public class ListParams
    {
        public string Filter { get; set; } = string.Empty;
        public string Fields { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty;
        public string SortField { get; set; } = string.Empty;
        public bool ShowDeleted { get; set; } = false;
        public int Take { get; set; } = 50;
        public int Skip { get; set; } = 0;
    }
}
