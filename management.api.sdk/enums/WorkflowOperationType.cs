namespace agility.enums
{
    /// <summary>
    /// Workflow operation types specifically for batch processing.
    /// Subset of BatchOperationType containing only workflow-related operations.
    /// </summary>
    public enum WorkflowOperationType
    {
        /// <summary>
        /// Publish the items in the batch.
        /// </summary>
        Publish = 1,

        /// <summary>
        /// Unpublish the items in the batch.
        /// </summary>
        Unpublish = 2,

        /// <summary>
        /// Approve the items in the batch.
        /// </summary>
        Approve = 3,

        /// <summary>
        /// Decline the items in the batch.
        /// </summary>
        Decline = 4,

        /// <summary>
        /// Request approval for the items in the batch.
        /// </summary>
        RequestApproval = 5
    }
}
