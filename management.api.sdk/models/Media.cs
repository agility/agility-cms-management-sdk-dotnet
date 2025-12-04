using System.Text.Json.Serialization;

namespace agility.models
{
    public class Media
    {
		private string _originKey;

		public bool HasChildren { get; set; }

		public int MediaID { get; set; }
		// Alias for MediaID to support camelCase naming convention
		public int mediaID { get => MediaID; set => MediaID = value; }
		public string FileName { get; set; }

		public int ContainerID { get; set; }
		public string ContainerOriginUrl { get; set; }
		public string ContainerEdgeUrl { get; set; }

		public string OriginKey
		{
			get { return (_originKey != null) ? _originKey.TrimStart('/') : null; }
			set { _originKey = value; }
		}
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? ModifiedBy { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? ModifiedByName { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public DateTime? DateModified { get; set; }

		public long Size { get; set; }
		public bool IsFolder { get; set; }
		public bool IsDeleted { get; set; }
		public int MediaGroupingID { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? MediaGroupingName { get; set; }
		public int MediaGroupingSortOrder { get; set; }
		public string ContentType { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? GridThumbnailID { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? GridThumbnailSuffix { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public System.Int64? rowNumber { get; set; }


		/// <summary>
		/// Allows calling methods to indicate whether a Media object is to be used as a thumbnail.
		/// </summary>
		internal bool ForThumbnail { get; set; }
		public string OriginUrl
		{
			get
			{

				string url = ContainerOriginUrl;
				if (!url.EndsWith("/"))
				{
					url = string.Format("{0}/{1}", url, OriginKey);
				}
				else
				{
					url = string.Format("{0}{1}", url, OriginKey);
				}
				return url;
			}
		}
		public string EdgeUrl
		{
			get
			{

				string url = ContainerEdgeUrl;
				if (string.IsNullOrEmpty(url)) return OriginUrl;

				if (!url.EndsWith("/"))
				{
					url = string.Format("{0}/{1}", url, OriginKey);
				}
				else
				{
					url = string.Format("{0}{1}", url, OriginKey);
				}

				return url;
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public bool? IsImage
		{
			get
			{
				int dotIndex = OriginKey.LastIndexOf(".");
				if (dotIndex == -1)
				{
					return false;
				}
				string ext = OriginKey.Substring(OriginKey.LastIndexOf("."));

				switch (ext.ToLowerInvariant())
				{
					case ".jpg":
					case ".jpeg":
					case ".gif":
					case ".png":
					case ".bmp":
					case ".svg":
					case ".avif":
					case ".webp":
						return true;
					default:
						return false;

				}
			}
		}

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public bool? IsSvg
		{
			get
			{
				int dotIndex = OriginKey.LastIndexOf(".");
				if (dotIndex == -1)
				{
					return false;
				}
				string ext = OriginKey.Substring(OriginKey.LastIndexOf("."));

				return ext.ToLowerInvariant() == ".svg";
			}
		}
	}

	public class AssetMediaList
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public List<Media?> assetMedias { get; set; }
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? totalCount { get; set; }
	}
}
