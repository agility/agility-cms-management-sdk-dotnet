

namespace agility.models
{
    public class BatchItem
    {
		public int? BatchItemID { get; set; }
		public int? BatchID { get; set; }
		public string? ItemTypeName { get; set; }

		public int ItemID { get; set; }
		public string? LanguageCode { get; set; }
		public int? ProcessedItemVersionID { get; set; }
		public string? ItemTitle { get; set; }

		public bool ItemNull { get; set; }
		public DateTime CreatedOn { get; set; }
		public int CreatedBy { get; set; }
		public string? CreatedByName { get; set; }
	}
}
