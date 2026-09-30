# Content

`instance.Content` works with content items. Every method takes the locale first. Saves, deletes and workflow
operations run as [batches](concepts.md#batches) and return a `BatchResult`.

## Read

```csharp
ContentItem item = await instance.Content.GetContentItemAsync("en-us", 42);

string? title = item.Fields?["title"]?.GetValue<string>();
int? state = item.Properties?.State;        // see ItemState
```

`Fields` is a `JsonObject` keyed by field name (camelCase), holding whatever the model defines: strings,
numbers, linked content, image objects. Read values with `GetValue<T>()`, or deserialize a field into your own
type with `field.Deserialize<T>()`.

Several items at once:

```csharp
List<ContentItem> items = await instance.Content.GetContentItemsByIdAsync("en-us", [42, 43, 44]);
```

## List and filter

`GetContentListAsync` lists the items in a container (by its reference name), a page at a time:

```csharp
ContentList page = await instance.Content.GetContentListAsync("en-us", "blogposts", take: 50, skip: 0,
    sortField: "title", sortDirection: "asc");

Console.WriteLine($"{page.TotalCount} items");
foreach (JsonNode? row in page.Items ?? [])
    Console.WriteLine(row?["contentID"]);
```

Filter with a `ContentListFilterModel`:

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
var recent = await instance.Content.GetContentListAsync("en-us", "blogposts", filter, take: 20);
```

`fields: "title,category"` returns only those fields, which keeps large lists fast. `showDeleted: true` includes
deleted items.

## Save

Read the whole item, change it, and send it all back: a save [replaces the item](concepts.md#saves-replace-the-whole-item).

```csharp
var item = await instance.Content.GetContentItemAsync("en-us", 42);
item.Fields!["title"] = "New title";
BatchResult saved = await instance.Content.SaveContentItemAsync("en-us", item);
```

To create an item, use a `ContentID` of `-1` and name the container and model:

```csharp
var created = await instance.Content.SaveContentItemAsync("en-us", new ContentItem
{
    ContentID = -1,
    Properties = new ContentItemProperties { ReferenceName = "blogposts", DefinitionName = "BlogPost" },
    Fields = new JsonObject { ["title"] = "Hello", ["body"] = "<p>First post</p>" },
});
int newId = created.ItemId!.Value;
```

Several items in one batch:

```csharp
BatchResult result = await instance.Content.SaveContentItemsAsync("en-us", [first, second, third]);
IReadOnlyList<int> ids = result.ItemIds;   // in the same order
```

If any item fails, the call throws `AgilityBatchException`; `ex.Batch.Items` shows which items succeeded.

A save always lands in Staging. [Publish](#workflow) for the change to go live.

## Workflow

```csharp
await instance.Content.PublishContentItemAsync("en-us", 42, comments: "Launch");
await instance.Content.UnpublishContentItemAsync("en-us", 42);
await instance.Content.RequestApprovalContentItemAsync("en-us", 42);
await instance.Content.ApproveContentItemAsync("en-us", 42);
await instance.Content.DeclineContentItemAsync("en-us", 42, comments: "Needs a new image");
await instance.Content.DeleteContentItemAsync("en-us", 42);
```

Many items with one operation, in one batch:

```csharp
await instance.Content.BatchWorkflowContentItemsAsync("en-us", [42, 43, 44], WorkflowOperationType.Publish);
```

### Publishing what an item depends on

An item can link to other content and nested lists. `GetCascadeItemsAsync` shows the tree that a cascade publish
would include, and `PublishContentItemCascadeAsync` publishes it:

```csharp
CascadeItem tree = await instance.Content.GetCascadeItemsAsync("en-us", 42);
BatchCreateResult created = await instance.Content.PublishContentItemCascadeAsync("en-us", 42);
foreach (var batchId in created.BatchIDs ?? [])
    await instance.Batches.WaitForBatchAsync(batchId);
```

## History and comments

```csharp
ContentItemHistoryResponse history = await instance.Content.GetContentItemHistoryAsync("en-us", 42, take: 20);
ItemCommentsResponse comments = await instance.Content.GetContentItemCommentsAsync("en-us", 42);
```

## Custom batches

To group operations on pages and content items yourself, create a batch:

```csharp
BatchCreateResult batch = await instance.Batches.CreateBatchAsync(new CreateBatchWithItemsRequest
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
