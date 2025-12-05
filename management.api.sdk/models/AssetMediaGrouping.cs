using agility.enums;

namespace agility.models
{
    public class AssetMediaGrouping
    {
        public int? groupingID { get; set; }
        public AssetGroupingType? groupingType { get; set; }

		public int? groupingTypeID
		{
			get
			{
				return (int?)groupingType;
			}
			set
			{
				groupingType = (AssetGroupingType?)value;
			}
		}
		public string? name { get; set; }
		public string? description { get; set; }
		public int? modifiedBy { get; set; }
		public string? modifiedByName { get; set; }
		public DateTime? modifiedOn { get; set; }
		public bool isDeleted { get; set; }
		public bool isFolder { get; set; }

		Dictionary<string, AssetMediaGroupingMetaData>? _metaData = null;

		public Dictionary<string, AssetMediaGroupingMetaData> metaData
		{
			get
			{
				if (_metaData == null) _metaData = new Dictionary<string, AssetMediaGroupingMetaData>();
				return _metaData;
			}
			set { _metaData = value; }
		}
	}
}
