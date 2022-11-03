using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace agility.models
{
    public class ContentItem
    {
		public int contentID { get; set; }
		public ContentItemProperties properties { get; set; } = new ContentItemProperties();
		public Dictionary<string, object> fields { get; set; } = new Dictionary<string, object>();
		public SeoProperties? seo { get; set; }
		public List<ContentTag?>? tags { get; set; }
		public ContentScripts? scripts { get; set; }

		public object? GetField(string fieldName, Type type)
		{
			if (properties != null
				&& fieldName.ToLower() == "itemorder")
			{
				return properties.itemOrder;
			}

			// Convert fieldName to camel case because these names are coming from json during merge.
			fieldName = char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);

			if (fields != null && fields.TryGetValue(fieldName, out var val))
			{
				if (val == null) return null;

				if (val.GetType() == type)
				{
					return val;
				}

				switch (type.Name)
				{
					case nameof(DateTime):
						{
							if (DateTime.TryParse(val.ToString(), out var result))
							{
								return result;
							}
						}
						break;
					case nameof(Int32):
						{
							if (int.TryParse(val.ToString(), out var result))
							{
								return result;
							}
						}
						break;


					case nameof(Decimal):
						{
							if (decimal.TryParse(val.ToString(), out var result))
							{
								return result;
							}
						}
						break;
					case nameof(Double):
						{
							if (double.TryParse(val.ToString(), out var result))
							{
								return result;
							}
						}
						break;
					case nameof(Boolean):
						{
							if (bool.TryParse(val.ToString(), out var result))
							{
								return result;
							}
						}
						break;

					default:
						return val.ToString();

				}

				// TODO: Decide if we want to convert to a type
				return null;
			}

			return null;
		}
	}
	public class ContentItemProperties
	{
		public int? state { get; set; }
		public int? modifiedBy { get; set; }
		public DateTime? modified { get; set; }
		public DateTime? pullDate { get; set; }
		public DateTime? releaseDate { get; set; }
		public int? versionID { get; set; }
		public string? referenceName { get; set; }
		public string? definitionName { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? itemOrder { get; set; }


	}


	public class SeoProperties
	{
		public string? metaDescription { get; set; }
		public string? metaKeywords { get; set; }
		public string? metaHTML { get; set; }

		public bool? menuVisible { get; set; }
		public bool? sitemapVisible { get; set; }
	}

	public class ContentTag
	{
		public int? contentTagID { get; set; }
		public int? tagContainerID { get; set; }
		public string? tag { get; set; }
	}

	public class ContentScripts
	{
		public string? top { get; set; }
		public string? bottom { get; set; }
	}
}
