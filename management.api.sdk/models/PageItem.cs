namespace agility.models
{
    public class PageItem
    {
		public int? pageID { get; set; }
		public string? name { get; set; }
		public string? path { get; set; }
		public string? title { get; set; }
		public string? menuText { get; set; }
		public string? pageType { get; set; }
		public string? templateName { get; set; }
		public string? redirectUrl { get; set; }
		public bool? securePage { get; set; }
		public bool? excludeFromOutputCache { get; set; }
		public PageVisible? visible { get; set; }
		public SeoProperties? seo { get; set; }
		public PageScripts? scripts { get; set; }
		public PageDynamic? dynamic { get; set; }
		public PageItemProperties? properties { get; set; }
		public Dictionary<string, List<PageModule?>>? zones { get; set; }
		public int? parentPageID { get; set; }
		public int? placeBeforePageItemID { get; set; }
		public int? channelID { get; set; }
		public DateTime? releaseDate { get; set; }
		public DateTime? pullDate { get; set; }
	}

	public class PageVisible
	{
		public bool? menu { get; set; }
		public bool? sitemap { get; set; }
	}

	public class PageScripts
	{
		public bool excludedFromGlobal { get; set; }
		public string? top { get; set; }
		public string? bottom { get; set; }
	}

	public class PageDynamic
	{
		public string? referenceName { get; set; }
		public string? fieldName { get; set; }

		public string? titleFormula { get; set; }
		public string? menuTextFormula { get; set; }
		public string? pageNameFormula { get; set; }

		public bool? visibleOnMenu { get; set; }
		public bool? visibleOnSitemap { get; set; }
	}

	public class PageItemProperties
	{
		public int? state { get; set; }
		public DateTime? modified { get; set; }
		public int? versionID { get; set; }
	}

	public class PageModule
	{
		public string? module { get; set; }
		public object? item { get; set; }
	}
}
