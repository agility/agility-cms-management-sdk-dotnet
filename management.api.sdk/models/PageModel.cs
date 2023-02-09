namespace agility.models;
public class PageModel
{
    public bool DoesPageTemplateHavePages { get; set; }
    public int? PageTemplateID { get; set; }

    public int? DigitalChannelTypeID { get; set; }

    public string? DigitalChannelTypeName { get; set; }

    public bool AgilityCode { get; set; }

    public string? PageTemplateName { get; set; }

    public string? RelativeURL { get; set; }

    public string? PageNames { get; set; }

    public bool IsDeleted { get; set; }

    public string? PreviewUrl { get; set; }

    public List<ContentSectionDefinition?>? ContentSectionDefinitions { get; set; }
}
