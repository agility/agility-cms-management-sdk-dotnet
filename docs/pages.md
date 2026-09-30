# Pages and page templates

`instance.Pages` works with pages, the sitemap and page templates (page models). Page saves and workflow
operations run as [batches](concepts.md#batches), like content.

## Sitemap and pages

```csharp
List<Sitemap> sitemap = await instance.Pages.GetSitemapAsync("en-us");   // one entry per channel
PageItem page = await instance.Pages.GetPageAsync("en-us", 7);

// Components on the page, by zone name.
foreach (var (zone, modules) in page.Zones ?? [])
    Console.WriteLine($"{zone}: {modules.Count} components");
```

## Save a page

As with content, read the whole page, change it, and send it back:

```csharp
var page = await instance.Pages.GetPageAsync("en-us", 7);
page.Title = "About us";
await instance.Pages.SavePageAsync("en-us", page);
await instance.Pages.PublishPageAsync("en-us", 7);
```

For a new page, use a `PageID` of `-1`. `SavePageAsync` also takes:

| Parameter | |
|---|---|
| `parentPageId` | the parent page; omit for the root |
| `placeBeforePageId` | the sibling to put the page before; omit for last |
| `otherLocale`, `pageIdInOtherLocale` | link the page to the same page in another locale |
| `linkExistingComponents` | link the page's components to existing content rather than copying it |

`result.ItemId` is the page ID.

## Workflow

`PublishPageAsync`, `UnpublishPageAsync`, `ApprovePageAsync`, `DeclinePageAsync`, `RequestApprovalPageAsync` and
`DeletePageAsync` work like their [content equivalents](content.md#workflow). `BatchWorkflowPagesAsync` runs one
operation on many pages in a single batch. `GetCascadeItemsAsync` and `PublishPageCascadeAsync` publish a page with
the content its components use.

`GetPageHistoryAsync` and `GetPageCommentsAsync` return the page's version history and comments.

## Page templates

A page template (page model) defines a page's zones. Each zone (`ContentSectionDefinition`) can have default
components (`DefaultModules`) that new pages get automatically.

```csharp
List<PageModel> templates = await instance.Pages.GetPageTemplatesAsync("en-us", includeModuleZones: true);
PageModel template = await instance.Pages.GetPageTemplateAsync("en-us", templateId);
PageModel byName = await instance.Pages.GetPageTemplateByNameAsync("en-us", "Main Template");
```

### Saving a template

Read [how the API reads zones](concepts.md#page-templates-zones-and-default-components) first: the zone list you
send is complete, and a zone's `DefaultModules` list replaces its default components.

Rename a template without touching its zones:

```csharp
var template = await instance.Pages.GetPageTemplateAsync("en-us", templateId);
template.PageTemplateName = "Landing Page";
template.ContentSectionDefinitions = null;   // keep the zones as they are
await instance.Pages.SavePageTemplateAsync("en-us", template);
```

Add a zone, keeping the others:

```csharp
var template = await instance.Pages.GetPageTemplateAsync("en-us", templateId);   // has every zone and its defaults
template.ContentSectionDefinitions!.Add(new ContentSectionDefinition
{
    PageItemTemplateID = -1,
    PageItemTemplateName = "Sidebar",
    PageItemTemplateReferenceName = "Sidebar",
});
await instance.Pages.SavePageTemplateAsync("en-us", template);
```

Clear one zone's default components:

```csharp
template.ContentSectionDefinitions!.Single(z => z.PageItemTemplateReferenceName == "Sidebar").DefaultModules = [];
await instance.Pages.SavePageTemplateAsync("en-us", template);
```

`GetPageTemplateZonesAsync` returns just the zones, but always with empty `DefaultModules`. Don't use it as the
source for a save.

`DeletePageTemplateAsync` deletes a template.
