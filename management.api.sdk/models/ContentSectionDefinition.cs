using agility.enums;
using System.Text.Json.Serialization;
namespace agility.models;
public class ContentSectionDefinition
{
    /// <summary>
    /// Default components for this zone. When saving a page template, null (the default) leaves the
    /// zone's existing defaults unchanged, a list replaces them, and an empty list clears them.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ContentSectionDefaultModule>? DefaultModules { get; set; }

    /// <summary>
    /// Shared components for this zone. Null (the default) is omitted from the request body.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SharedModule>? SharedModules { get; set; }

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
