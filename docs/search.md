# Search

`client.Search` searches everything in an instance at once: content items, components, pages, assets, models and
containers. Results are ranked across kinds, best match first, and come with a count per kind.

```csharp
SearchResult result = await client.Search.SearchAsync(guid, "newsletter");

Console.WriteLine($"{result.Total} results");
foreach (var hit in result.Hits ?? [])
    Console.WriteLine($"{hit.TypeLabel}: {hit.Name} ({hit.Context}) → {hit.ViewLink}");
```

Words match as prefixes (`newsl` finds "newsletter") and every word must match. Text in double quotes is an exact
phrase, and a number also matches content, page, asset and model IDs exactly. When nothing matches every word, the
results match any of them and `Relaxed` is `true`.

You only get what your permissions allow: components and models need Develop or Manage, assets need Edit or
Delete, and content and pages in containers or pages you can't read are left out.

## Filters and paging

`SearchOptions` narrows the search and picks a page of results:

```csharp
SearchResult posts = await client.Search.SearchAsync(guid, "\"spring sale\"", new SearchOptions
{
    Kinds = [SearchKinds.Content, SearchKinds.Page],
    Locale = "en-us",
    Containers = ["posts"],
    State = "published",
    ModifiedAfter = DateTime.UtcNow.AddDays(-30),
    Skip = 25,
    Take = 25,
});
```

| Option | |
|---|---|
| `Kinds` | Any of `SearchKinds`: `content`, `component`, `page`, `asset`, `model`, `container`. Default: every kind you can see. |
| `Locale` | Only content, components and pages in this locale. Default (or `*`): every locale. |
| `Containers`, `Models` | Only content in these containers, or of these models, by reference name. |
| `State` | `staging`, `published`, `unpublished` or `any`. |
| `ModifiedAfter` | Only results modified on or after this time. |
| `Skip`, `Take` | Paging. `Take` defaults to 25; the API returns at most 100. |

`Containers`, `Models` and `State` apply to content items and components only, so setting any of them limits the
search to those kinds. An unknown kind or state is a 400 (`AgilityManagementException`).

## The result

| Property | |
|---|---|
| `Hits` | The requested page of results. Each `SearchHit` has a `Kind`, a `Name`, a `Snippet` of the matching text (HTML-encoded, matched words in `<mark>`), the `MatchedFields`, a `ViewLink` into the Content Manager, and the IDs that fit its kind (`ContentID`, `PageID`, `MediaID`, `ModelID`, `ContainerID`, ...). |
| `Total` | Results across the requested kinds. |
| `Facets` | A count per kind you can see, keyed by kind, *whatever kinds you asked for*, so it can drive tabs. When `Capped` is `true` there are more than `Count`; show it as "N+". |
| `Indexing` | How far the index is behind. Changes are searchable a few seconds after a save, so `Pending` is usually 0. After a big import it's higher, and new or edited items may be missing until it reaches 0; `CatchingUp` means they're being indexed, so search again shortly. |

## When search isn't available

A few instances don't have search yet. For those the API returns 501 and the SDK throws
`AgilitySearchNotAvailableException`, a subclass of `AgilityManagementException`. It isn't retried: it's a property
of the instance, not a passing failure.

```csharp
try
{
    var found = await client.Search.SearchAsync(guid, "newsletter");
}
catch (AgilitySearchNotAvailableException)
{
    // Search isn't available for this instance yet. List content instead.
}
```
