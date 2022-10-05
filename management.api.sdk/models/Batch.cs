using agility.enums;

namespace agility.models
{
    public class Batch
    {
		public int? BatchID { get; set; }
		public int? BatchItemID { get; set; }
		public string BatchName { get; set; }
		public BatchState BatchState { get; set; }
		public BatchOperationType OperationType { get; set; }
		public int? OwnerUserID { get; set; }

		public DateTime CreatedDate { get; set; }
		public DateTime ModifiedDate { get; set; }
		public DateTime? ProcessDate { get; set; }
		public DateTime? QueueDate { get; set; }

		public bool IsPrivate { get; set; }

		public int ModifiedByUserID { get; set; }
		public int? QueuedByUserID { get; set; }

		public string OwnerName { get; set; }
		public string ModifiedByName { get; set; }
		public string QueuedByName { get; set; }

		public string AdditionalData { get; set; }

		public string ErrorData { get; set; }

		public int? PercentComplete { get; set; }
		public int? NumItemsProcessed { get; set; }
		public bool? AbortYN { get; set; }

		public string StatusMessage { get; set; }

		public int BatchItemCount { get; set; }

		public List<BatchItem> Items { get; set; }
	}
}
