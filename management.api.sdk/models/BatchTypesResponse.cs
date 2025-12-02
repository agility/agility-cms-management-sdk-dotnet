namespace agility.models
{
    /// <summary>
    /// Response model containing all batch-related enum types for developer discovery.
    /// </summary>
    public class BatchTypesResponse
    {
        /// <summary>
        /// Available item types that can be added to batches.
        /// </summary>
        public List<EnumInfo>? ItemTypes { get; set; }

        /// <summary>
        /// All available batch operation types (includes workflow and other operations).
        /// </summary>
        public List<EnumInfo>? OperationTypes { get; set; }

        /// <summary>
        /// Workflow-specific operation types (subset of operationTypes).
        /// </summary>
        public List<EnumInfo>? WorkflowOperations { get; set; }

        /// <summary>
        /// Available batch states during lifecycle.
        /// </summary>
        public List<EnumInfo>? States { get; set; }
    }

    /// <summary>
    /// Information about an enum value including value and name.
    /// </summary>
    public class EnumInfo
    {
        /// <summary>
        /// The numeric value of the enum.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The string name of the enum value (e.g., "ContentItem", "Publish").
        /// </summary>
        public string? Name { get; set; }
    }
}
