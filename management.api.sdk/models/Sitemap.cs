namespace agility.models
{
    public class Sitemap
    {
        public int? DigitalChannelID { get; set; }
        public string? Name { get; set; }
        public string? DigitalChannelTypeName { get; set; }
        public bool IsDefaultChannel { get; set; }
        public List<SitemapItem?> Pages { get; set; } = new List<SitemapItem?>();
    }

    public class SitemapItem
    {
        public int? PageID { get; set; }
        public int? ParentPageID { get; set; }
        public string? PageName { get; set; }
        public string? Title { get; set; }
        public string? PageType { get; set; }
        public string? LanguageCode { get; set; }
        public int? PageMode { get; set; }
        public string? DynamicPageContentReferenceName { get; set; }
        public string? DynamicPageContentViewFieldName { get; set; }
        public string? URL { get; set; }
        public List<SitemapItem?> ChildPages { get; set; } = new List<SitemapItem?>();
    }
}
