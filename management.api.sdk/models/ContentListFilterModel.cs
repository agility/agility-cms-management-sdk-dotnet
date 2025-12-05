namespace agility.models
{
    /// <summary>
    /// Filter model for content list queries.
    /// </summary>
    public class ContentListFilterModel
    {
        public List<int>? SortIDs { get; set; }
        public List<int>? ModifiedByIds { get; set; }
        public DateRangeFilter? DateRange { get; set; }
        public List<int>? StateIds { get; set; }
        public List<FieldFilter>? FieldFilters { get; set; }
        public string? GenericSearch { get; set; }
    }

    /// <summary>
    /// Date range filter for queries.
    /// </summary>
    public class DateRangeFilter
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }

    /// <summary>
    /// Numeric range filter for queries.
    /// </summary>
    public class NumRangeFilter
    {
        public int FromNum { get; set; }
        public int ToNum { get; set; }
    }

    /// <summary>
    /// Field filter value supporting multiple types.
    /// </summary>
    public class FieldFilterValue
    {
        public string? StringValue { get; set; }
        public DateRangeFilter? DateRangeValue { get; set; }
        public NumRangeFilter? NumRangeValue { get; set; }
        public bool? BoolValue { get; set; }
    }

    /// <summary>
    /// Field-based filter for content queries.
    /// </summary>
    public class FieldFilter
    {
        public string? Field { get; set; }
        public FieldFilterValue? Value { get; set; }
    }
}
