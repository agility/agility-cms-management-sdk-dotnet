# Migrating from 1.x to 2.0

2.0 is a rewrite. The API it calls is the same, but the SDK's shape changed: one consistent argument order, async
methods with cancellation, models named after the API's schemas, typed errors, and saves that wait for their
batch. This guide maps each 1.x call to its 2.0 replacement.

**If you can't move to .NET 10 yet,** stay on 1.x (it targets .NET 6, which .NET 6–9 projects can use). The 1.x
line gets fixes only, including the fix for `SavePageTemplate` clearing default components.

## Setup

```csharp
// 1.x
var clientInstance = new ClientInstance(new Options { token = token });
var item = await clientInstance.contentMethods.GetContentItem(42, guid, "en-us");

// 2.0
using var client = new AgilityManagementClient(new AgilityManagementOptions { AccessToken = token });
var item = await client.Content.GetContentItemAsync(guid, "en-us", 42);
```

- Namespaces: `management.api.sdk` is now `Agility.Management.Sdk` (the area clients are in
  `Agility.Management.Sdk.Clients`), and `agility.models` and `agility.enums` are both `Agility.Management.Sdk.Models`.
- Every instance-level method takes the arguments in one order: instance GUID, then locale (where the route has
  one), then IDs, then optional settings. 1.x mixed `(id, guid, locale)` and `(guid, locale, id)`.
- The `*Methods` properties become area properties: `contentMethods` → `client.Content`, `pageMethods` →
  `client.Pages`, and so on.
- The three methods with many optional settings take an options object: `GetContentListAsync`
  (`ContentListOptions`), `SavePageAsync` (`SavePageOptions`) and `GetContainerListPagedAsync` (`ContainerListOptions`).
- Every method ends in `Async` and takes a `CancellationToken`.
- With dependency injection, use `services.AddAgilityManagement(...)`.

### Options

| 1.x `Options` | 2.0 `AgilityManagementOptions` |
|---|---|
| `token` | `AccessToken` |
| `refresh_token` (unused in 1.x) | `RefreshToken`: the client now uses it to get and renew access tokens |
| `baseUrl` (ignored in 1.x) | `BaseUrl`: now honoured |
| `duration` (ms between batch checks) | `BatchPolling.Interval` (a `TimeSpan`) |
| `retryCount` (batch checks) | `BatchPolling.Timeout` (a `TimeSpan`, default 15 minutes) |
| `Local` / `BaseLocalURL` environment variables | `BaseUrl` |

## Behaviour changes

| Area | 1.x | 2.0 |
|---|---|---|
| Page template saves | Sent every zone's default components as `[]`, clearing them | Leaves `DefaultModules` out unless you set it. A read-then-save keeps them. |
| Batch operations | Returned the first item's ID, or threw `ApplicationException` on timeout | Return a `BatchResult` (batch ID, every item ID, the batch). Failed items throw `AgilityBatchException`. |
| Batch wait | Stopped at half the configured budget (the counter was decremented twice per check) | Waits the full `BatchPolling.Timeout` |
| Errors | `ApplicationException` with a message | `AgilityManagementException` with status code, API message, body and request ID |
| Retries | None | Reads are retried on transient failures; writes never are |
| Content lists | Called a hidden `GET` route that ignored `filter` | Calls the documented `POST` route with a `ContentListFilterModel` |
| USA 2 region (`-us2`) | Sent to the USA host | Sent to `mgmt-usa2.aglty.io` |
| Unknown GUID suffix | Silently used the USA host | Throws `ArgumentException` |
| Query values | Not URL-encoded | Encoded (fixes emails with `+`, folders with spaces) |
| Async | Blocked on `.Result` inside `async` methods | Fully asynchronous |

## Models

Models are generated from the API's OpenAPI schemas, so some names changed, and properties are PascalCase.

| 1.x | 2.0 |
|---|---|
| `Container` | `ContentContainer` |
| `Model`, `ModelField` | `ContentModel`, `ContentModelField` |
| `Media` | `AssetMedia` |
| `PagedResult<Container>` | `ContentContainerPagedResult` |
| `ContentItem.contentID`, `.properties`, `.fields` | `ContentItem.ContentID`, `.Properties`, `.Fields` |
| `ContentItem.fields` (`Dictionary<string, object>`) | `ContentItem.Fields` (`JsonObject`) |
| `ContentItem.GetField(name, type)` | `item.Fields?[name]?.GetValue<T>()` |
| `ContentList.items` (`ArrayList`) | `ContentList.Items` (`List<JsonNode>`) |

Collections on models are `null` until you set them (1.x initialised some to empty lists). Before adding to one on a
new object, create it: `zone.DefaultModules ??= [];`. Leaving a collection `null` means "don't change it"; see
[concepts](docs/concepts.md#unset-null-and-empty-collections).

## Method map

Every 2.0 method below also takes the instance GUID as its first argument (shown as `guid`). Methods marked † return a `BatchResult` instead of an ID: use
`result.ItemId` for the old return value.

### Assets (`assetMethods` → `client.Assets`)

| 1.x | 2.0 |
|---|---|
| `Upload(files, guid, folderPath, groupingID)` | `UploadAsync(guid, folderPath, [new AssetUpload(name, stream)], galleryId)` |
| `CreateFolder(originKey, guid)` | `CreateFolderAsync(guid, originKey)` |
| `DeleteFile(mediaID, guid)` | `DeleteAssetAsync(guid, mediaId)` |
| `MoveFile(mediaID, newFolder, guid)` | `MoveAssetAsync(guid, mediaId, newFolder)` |
| `GetMediaList(pageSize, recordOffset, guid)` | `GetMediaListAsync(guid, pageSize, recordOffset)` |
| `GetGalleries(guid, search, pageSize, rowIndex)` | `GetGalleriesAsync(guid, search, pageSize, rowIndex)` |
| `GetGalleryById(guid, id)` | `GetGalleryAsync(guid, galleryId)` |
| `GetGalleryByName(guid, name)` | `GetGalleryByNameAsync(guid, galleryName)` |
| `GetDefaultContainer(guid)` | `GetDefaultContainerAsync(guid)` |
| `SaveGallery(guid, gallery)` | `SaveGalleryAsync(guid, gallery)` |
| `DeleteGallery(guid, id)` | `DeleteGalleryAsync(guid, galleryId)` |
| `GetAssetByID(mediaID, guid)` | `GetAssetAsync(guid, mediaId)` |
| `GetAssetByURL(url, guid)` | `GetAssetByUrlAsync(guid, url)` |

### Batches (`batchMethods` → `client.Batches`)

| 1.x | 2.0 |
|---|---|
| `GetBatch(id, guid)` | `GetBatchAsync(guid, batchId)` |
| `Retry(func)` | `WaitForBatchAsync(guid, batchId)`; batch operations wait on their own |
| `contentMethods.GetBatchObject` / `pageMethods.GetBatchObject` | `GetBatchAsync(guid, batchId)` |

### Containers (`containerMethods` → `client.Containers`)

| 1.x | 2.0 |
|---|---|
| `GetContainerById(id, guid)` | `GetContainerAsync(guid, containerId)` |
| `GetContainerByReferenceName(name, guid)` | `GetContainerByReferenceNameAsync(guid, referenceName)` |
| `GetContainersByModel(modelId, guid)` | `GetContainersByModelAsync(guid, modelId)` |
| `GetContainerSecurity(id, guid)` | `GetContainerSecurityAsync(guid, containerId)` |
| `GetContainerList(guid)` | `GetContainerListAsync(guid)` |
| `GetContainerListPaged(guid, ...)` | `GetContainerListPagedAsync(guid, new ContainerListOptions { ... })` |
| `GetNotificationList(id, guid)` | `GetNotificationsAsync(guid, containerId)` |
| `SaveContainer(container, guid)` | `SaveContainerAsync(guid, container)` |
| `DeleteContainer(id, guid)` | `DeleteContainerAsync(guid, containerId)` |

### Content (`contentMethods` → `client.Content`)

| 1.x | 2.0 |
|---|---|
| `GetContentItem(contentID, guid, locale)` | `GetContentItemAsync(guid, locale, contentId)` |
| `GetContentItems(referenceName, guid, locale, filter, fields, sortDirection, sortField, take, skip)` | `GetContentListAsync(guid, locale, referenceName, new ContentListOptions { Filter, Take, Skip, Fields, SortField, SortDirection })` |
| `SaveContentItem(item, guid, locale)` † | `SaveContentItemAsync(guid, locale, item)` |
| `SaveContentItems(items, guid, locale)` | `SaveContentItemsAsync(guid, locale, items)`: `result.ItemIds`; failures throw instead of appearing as strings in the list |
| `DeleteContent(contentID, guid, locale, comments)` † | `DeleteContentItemAsync(guid, locale, contentId, comments)` |
| `PublishContent(...)` † | `PublishContentItemAsync(guid, locale, contentId, comments)` |
| `UnPublishContent(...)` † | `UnpublishContentItemAsync(guid, locale, contentId, comments)` |
| `ApproveContent(...)` † | `ApproveContentItemAsync(guid, locale, contentId, comments)` |
| `DeclineContent(...)` † | `DeclineContentItemAsync(guid, locale, contentId, comments)` |
| `ContentRequestApproval(...)` † | `RequestApprovalContentItemAsync(guid, locale, contentId, comments)` |

### Instance users (`instanceUserMethods` → `client.InstanceUsers`)

| 1.x | 2.0 |
|---|---|
| `GetUsers(guid)` | `GetUsersAsync(guid)` |
| `SaveUser(email, roles, guid, first, last)` | `SaveUserAsync(guid, email, roles, firstName, lastName)` |
| `DeleteUser(userID, guid)` | `DeleteUserAsync(guid, userId)` |

### Models (`modelMethods` → `client.Models`)

| 1.x | 2.0 |
|---|---|
| `GetContentModel(id, guid)` | `GetModelAsync(guid, modelId)` |
| `GetModelByReferenceName(name, guid)` | `GetModelByReferenceNameAsync(guid, referenceName)` |
| `GetContentModules(includeDefaults, guid, includeModules)` | `GetContentModelsAsync(guid, includeDefaults, includeModules)` |
| `GetPageModules(guid, includeDefault)` | `GetComponentModelsAsync(guid, includeDefault)` |
| `SaveModel(model, guid)` | `SaveModelAsync(guid, model)` |
| `DeleteModel(id, guid)` | `DeleteModelAsync(guid, modelId)` |

### Pages (`pageMethods` → `client.Pages`)

| 1.x | 2.0 |
|---|---|
| `GetSiteMap(guid, locale)` | `GetSitemapAsync(guid, locale)` |
| `GetPage(pageID, guid, locale)` | `GetPageAsync(guid, locale, pageId)` |
| `SavePage(page, guid, locale, parentPageID, placeBeforePageItemID, pageIDInOtherLocale, otherLocale)` † | `SavePageAsync(guid, locale, page, new SavePageOptions { ParentPageId, PlaceBeforePageId, OtherLocale, PageIdInOtherLocale })`: settings left `null` use the API's defaults |
| `DeletePage(...)` † | `DeletePageAsync(guid, locale, pageId, comments)` |
| `PublishPage` / `UnPublishPage` / `ApprovePage` / `DeclinePage` / `PageRequestApproval` † | `PublishPageAsync` / `UnpublishPageAsync` / `ApprovePageAsync` / `DeclinePageAsync` / `RequestApprovalPageAsync`, each `(locale, pageId, comments)` |
| `GetPageTemplates(guid, locale, includeModuleZones, searchFilter)` | `GetPageTemplatesAsync(guid, locale, includeModuleZones, searchFilter)` |
| `GetPageTemplate(guid, locale, id)` | `GetPageTemplateAsync(guid, locale, pageTemplateId)` |
| `GetPageTemplateByName(guid, locale, name)` | `GetPageTemplateByNameAsync(guid, locale, templateName)` |
| `GetPageItemTemplates(guid, locale, id)` | `GetPageTemplateZonesAsync(guid, locale, pageTemplateId)` |
| `SavePageTemplate(guid, locale, template)` | `SavePageTemplateAsync(guid, locale, template)` |
| `DeletePageTemplate(guid, locale, id)` | `DeletePageTemplateAsync(guid, locale, pageTemplateId)` |

Delete methods that returned the API's message string now return `Task`; a failure throws.

## New in 2.0

Webhooks, locales, localization (initialize and translate), URL redirections, Personal Access Tokens, the current
user, batch actions and custom batches, batch workflow, cascade publish, history and comments, folder rename and
delete, Fetch API sync status and keys, and OAuth helpers. See [API coverage](docs/api-coverage.md).
