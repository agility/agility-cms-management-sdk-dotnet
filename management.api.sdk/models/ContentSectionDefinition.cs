using agility.enums;
namespace agility.models;
public class ContentSectionDefinition
{
    private List<ContentSectionDefaultModule> _defaultModules = new List<ContentSectionDefaultModule>();
    private List<SharedModule> _sharedModules = new List<SharedModule>();
    public List<ContentSectionDefaultModule> DefaultModules
    {
        get { return _defaultModules; }
    }
    public List<SharedModule> SharedModules
    {
        get { return _sharedModules; }
    }

    public int? PageItemTemplateID { get; set; }
    public int? PageTemplateID { get; set; }
    public string? PageItemTemplateName { get; set; }
    public string? PageItemTemplateReferenceName { get; set; }
    public PageItemTemplateType PageItemTemplateType { get; set; }
    public string? PageItemTemplateTypeName { get; set; }
    public int? ItemOrder { get; set; }
    public int? ModuleOrder { get; set; }
    public int? ContentViewID { get; set; }
    public string? ContentReferenceName { get; set; }
    public int? ContentDefinitionID { get; set; }
    public string? ContentViewName { get; set; }
    public int? ItemContainerID { get; set; }
    public int? PublishContentItemID { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime PullDate { get; set; }
    public bool IsShared { get; set; }
    public bool IsSharedTemplate { get; set; }
    public bool EnablePersonalization { get; set; }
    public bool DoesPageTemplateHavePages { get; set; }
    public int? ModuleID { get; set; }
    public string? ContentDefinitionTitle { get; set; }
    public string? UserControlPath { get; set; }
    public string? TemplateMarkup { get; set; }
    public string? SortExpression { get; set; }
    public string? FilterExpression { get; set; }
    public int? ContentTemplateID { get; set; }
    public string? ContentTemplateName { get; set; }
}
