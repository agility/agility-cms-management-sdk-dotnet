namespace agility.enums
{
    /// <summary>
    /// Types of items that can be included in a batch.
    /// </summary>
    public enum BatchItemType
    {
        /// <summary>
        /// Page item.
        /// </summary>
        Page = 1,

        /// <summary>
        /// Content item.
        /// </summary>
        ContentItem = 2,

        /// <summary>
        /// Content list.
        /// </summary>
        ContentList = 3,

        /// <summary>
        /// Tag.
        /// </summary>
        Tag = 4,

        /// <summary>
        /// Module definition.
        /// </summary>
        ModuleDef = 5
    }
}
