using agility.enums;

namespace agility.models
{
    public class AssetMediaGrouping
    {
        public int? MediaGroupingID { get; set; }
        public AssetGroupingType? GroupingType { get; set; }

		public int? GroupingTypeID
		{
			get
			{
				return (int)GroupingType;
			}
			set
			{
				GroupingType = (AssetGroupingType)value;
			}
		}
		public string? Name { get; set; }
		public string? Description { get; set; }
		public int? ModifiedBy { get; set; }
		public string? ModifiedByName { get; set; }
		public DateTime? ModifiedOn { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsFolder { get; set; }

		Dictionary<string, AssetMediaGroupingMetaData> _metaData = null;

		public Dictionary<string, AssetMediaGroupingMetaData> MetaData
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
