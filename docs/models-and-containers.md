# Models and containers

A **model** defines fields. A **content model** is used by content items; a **component model** (module) is used by
page components. A **container** holds content items of one content model: a single item or a list.

## Models

```csharp
List<ContentModel> contentModels = await client.Models.GetContentModelsAsync(guid, includeDefaults: false);
List<ContentModel> componentModels = await client.Models.GetComponentModelsAsync(guid);
ContentModel model = await client.Models.GetModelByReferenceNameAsync(guid, "BlogPost");
ContentModel byId = await client.Models.GetModelAsync(guid, model.Id!.Value);
```

`GetContentModelsAsync` takes `includeModules: true` to include component models, and `updatedSince` to get only
recently changed models.

Create or update a model; use an `Id` of `0` to create one:

```csharp
var saved = await client.Models.SaveModelAsync(guid, new ContentModel
{
    Id = 0,
    DisplayName = "Blog Post",
    ReferenceName = "BlogPost",
    Fields =
    [
        new ContentModelField
        {
            Name = "Title", Label = "Title", Type = "Text", IsDataField = true, Editable = true,
            Settings = new() { ["Required"] = "True" },
        },
    ],
});
```

As with content, the field list you send is the model's complete field list. Read the model, change it and save
it back to add a field. `GetFieldTypesAsync` lists the field types you can use; `GetUsedFieldTypesAsync` lists the
ones the instance's models use, including custom fields.

`DeleteModelAsync` deletes a model.

## Containers

```csharp
List<ContentContainer> all = await client.Containers.GetContainerListAsync(guid);
ContentContainer posts = await client.Containers.GetContainerByReferenceNameAsync(guid, "blogposts");
List<ContentContainer> forModel = await client.Containers.GetContainersByModelAsync(guid, model.Id!.Value);
```

For large instances, page through them:

```csharp
ContentContainerPagedResult page = await client.Containers.GetContainerListPagedAsync(guid,
    new ContainerListOptions { PageSize = 100, RecordOffset = 0 });
```

Create a list container for a model:

```csharp
var container = await client.Containers.SaveContainerAsync(guid, new ContentContainer
{
    ContentViewID = 0,
    ContentDefinitionID = model.Id,
    ContentDefinitionTypeID = 1,
    ContentViewName = "Blog Posts",
    ReferenceName = "blogposts",
    IsShared = false,
    IsDynamicPageList = true,
});
```

The API may adjust a reference name to keep it unique; pass `forceReferenceName: true` to keep it exactly as
given.

`GetContainerSecurityAsync` returns a container with its user and team permissions, and `GetNotificationsAsync`
its notification settings. `DeleteContainerAsync` deletes a container.
