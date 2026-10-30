# Content

`client.Content` works with content items. Every method takes the instance GUID and the locale first. Saves, deletes and workflow
operations run as [batches](concepts.md#batches) and return a `BatchResult`.

## Read

```csharp
ContentItem item = await client.Content.GetContentItemAsync(guid, "en-us", 42);

string? title = item.Fields?["title"]?.GetValue<string>();
int? state = item.Properties?.State;        // see ItemState
```

`Fields` is a `JsonObject` keyed by field name (camelCase), holding whatever the model defines: strings,
numbers, linked content, image objects. Read values with `GetValue<T>()`, or deserialize a field into your own
type with `field.Deserialize<T>()`.

Several items at once:

```csharp
List<ContentItem> items = await client.Content.GetContentItemsByIdAsync(guid, "en-us", [42, 43, 44]);
```

## List and filter

`GetContentListAsync` lists the items in a container (by its reference name), a page at a time:

```csharp
ContentList page = await client.Content.GetContentListAsync(guid, "en-us", "blogposts",
    new ContentListOptions { Take = 50, Skip = 0, SortField = "title", SortDirection = "asc" });

Console.WriteLine($"{page.TotalCount} items");
foreach (JsonNode? row in page.Items ?? [])
    Console.WriteLine(row?["contentID"]);
```

Filter with a `ContentListFilterModel` in the options:

```csharp
var filter = new ContentListFilterModel
{
    GenericSearch = "launch",                         // free-text search
    StateIds = [(int)ItemState.Published],
    DateRange = new DateRangeFilter { StartDate = DateTime.UtcNow.AddDays(-30) },
    FieldFilters =
    [
        new FieldFilter { Field = "category", Value = new FieldFilterValue { StringValue = "news" } },
    ],
};
var recent = await client.Content.GetContentListAsync(guid, "en-us", "blogposts",
    new ContentListOptions { Filter = filter, Take = 20 });
```

`Fields = "title,category"` returns only those fields, which keeps large lists fast. `ShowDeleted = true` includes
deleted items.

### Order

`SetContentListItemOrderAsync` puts a list's items in the order you give, for one locale. Items you leave out keep
their relative order after the ones you list, so moving one item to the top takes just its ID:

```csharp
ListItemOrderResult order = await client.Content.SetContentListItemOrderAsync(guid, "en-us", "footer_links", [12]);
// order.ContentIDs: every item in its new order. order.ItemsUpdated: 0 if 12 was already first.
```

The order applies to the staging and the live list at once. No item gets a new version or changes state, so there's
nothing to publish, even for items with unpublished changes. Order is kept per locale: call it once per locale. A
content ID that isn't in the list, or one given twice, is a 400 and nothing changes.

## Save

Read the whole item, change it, and send it all back: a save [replaces the item](concepts.md#saves-replace-the-whole-item).

```csharp
var item = await client.Content.GetContentItemAsync(guid, "en-us", 42);
item.Fields!["title"] = "New title";
BatchResult saved = await client.Content.SaveContentItemAsync(guid, "en-us", item);
```

To create an item, use a `ContentID` of `-1` and name the container and model:

```csharp
var created = await client.Content.SaveContentItemAsync(guid, "en-us", new ContentItem
{
    ContentID = -1,
    Properties = new ContentItemProperties { ReferenceName = "blogposts", DefinitionName = "BlogPost" },
    Fields = new JsonObject { ["title"] = "Hello", ["body"] = "<p>First post</p>" },
});
int newId = created.ItemId!.Value;
```

Several items in one batch:

```csharp
BatchResult result = await client.Content.SaveContentItemsAsync(guid, "en-us", [first, second, third]);
IReadOnlyList<int> ids = result.ItemIds;   // in the same order
```

If any item fails, the call throws `AgilityBatchException`; `ex.Batch.Items` shows which items succeeded.

A save always lands in Staging. [Publish](#workflow) for the change to go live.

### GEO / AEO on dynamic page list items

An item in a list that backs a dynamic page carries its own `Geo` (`SchemaType`, `Summary`, `StructuredData`, `Ai`),
the same `GeoProperties` as a page's, with the same three states on save: unset keeps the stored value, a `Clear*`
method clears it, a value replaces it. `item.ClearGeo()` sends `"geo": null` to clear the whole block. See
[Pages](pages.md#geo--aeo-and-seo-properties) for the details. Items in other lists don't carry `Geo`.

```csharp
var post = await client.Content.GetContentItemAsync(guid, "en-us", 42);
post.Geo ??= new GeoProperties();
post.Geo.Summary = "The post in one sentence.";
post.Geo.Ai = new GeoAi { Crawl = "block" };
await client.Content.SaveContentItemAsync(guid, "en-us", post);

await client.Content.SaveContentItemAsync(guid, "en-us", post.ClearGeo());   // clear all of its GEO data
```

## Workflow

```csharp
await client.Content.PublishContentItemAsync(guid, "en-us", 42, comments: "Launch");
await client.Content.UnpublishContentItemAsync(guid, "en-us", 42);
await client.Content.RequestApprovalContentItemAsync(guid, "en-us", 42);
await client.Content.ApproveContentItemAsync(guid, "en-us", 42);
await client.Content.DeclineContentItemAsync(guid, "en-us", 42, comments: "Needs a new image");
await client.Content.DeleteContentItemAsync(guid, "en-us", 42);
```

Many items with one operation, in one batch:

```csharp
await client.Content.BatchWorkflowContentItemsAsync(guid, "en-us", [42, 43, 44], WorkflowOperationType.Publish);
```

### Publishing what an item depends on

An item can link to other content and nested lists. `GetCascadeItemsAsync` shows the tree that a cascade publish
would include, and `PublishContentItemCascadeAsync` publishes it:

```csharp
CascadeItem tree = await client.Content.GetCascadeItemsAsync(guid, "en-us", 42);
BatchCreateResult created = await client.Content.PublishContentItemCascadeAsync(guid, "en-us", 42);
foreach (var batchId in created.BatchIDs ?? [])
    await client.Batches.WaitForBatchAsync(guid, batchId);
```

## History and comments

```csharp
ContentItemHistoryResponse history = await client.Content.GetContentItemHistoryAsync(guid, "en-us", 42, take: 20);
ItemCommentsResponse comments = await client.Content.GetContentItemCommentsAsync(guid, "en-us", 42);
```

## Custom batches

To group operations on pages and content items yourself, create a batch:

```csharp
BatchCreateResult batch = await client.Batches.CreateBatchAsync(guid, new CreateBatchWithItemsRequest
{
    BatchName = "Spring launch",
    Operation = WorkflowOperationType.Publish,
    Items =
    [
        new AddBatchItemRequest { ItemType = BatchItemType.ContentItem, ItemID = 42, LanguageCode = "en-us" },
        new AddBatchItemRequest { ItemType = BatchItemType.Page, ItemID = 7, LanguageCode = "en-us" },
    ],
}, processNow: true);
```

`PublishBatchAsync`, `UnpublishBatchAsync`, `ApproveBatchAsync`, `DeclineBatchAsync` and
`RequestApprovalBatchAsync` apply an operation to every item in an existing batch.
