# Migrating from 1.x to 2.0

2.0 is a rewrite. The API it calls is the same, but the SDK's shape changed: an instance-scoped client, async
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
var instance = client.ForInstance(guid);
var item = await instance.Content.GetContentItemAsync("en-us", 42);
```

- Namespaces: `management.api.sdk` is now `Agility.Management.Sdk` (the area clients are in
  `Agility.Management.Sdk.Clients`), and `agility.models` and `agility.enums` are both `Agility.Management.Sdk.Models`.
- The instance GUID moves from every call to `client.ForInstance(guid)`. The locale is now the first parameter.
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

`instance` is `client.ForInstance(guid)`. Methods marked † return a `BatchResult` instead of an ID: use
`result.ItemId` for the old return value.

### Assets (`assetMethods` → `instance.Assets`)

| 1.x | 2.0 |
|---|---|
| `Upload(files, guid, folderPath, groupingID)` | `UploadAsync(folderPath, [new AssetUpload(name, stream)], galleryId)` |
| `CreateFolder(originKey, guid)` | `CreateFolderAsync(originKey)` |
| `DeleteFile(mediaID, guid)` | `DeleteAssetAsync(mediaId)` |
| `MoveFile(mediaID, newFolder, guid)` | `MoveAssetAsync(mediaId, newFolder)` |
| `GetMediaList(pageSize, recordOffset, guid)` | `GetMediaListAsync(pageSize, recordOffset)` |
| `GetGalleries(guid, search, pageSize, rowIndex)` | `GetGalleriesAsync(search, pageSize, rowIndex)` |
| `GetGalleryById(guid, id)` | `GetGalleryAsync(galleryId)` |
| `GetGalleryByName(guid, name)` | `GetGalleryByNameAsync(galleryName)` |
| `GetDefaultContainer(guid)` | `GetDefaultContainerAsync()` |
| `SaveGallery(guid, gallery)` | `SaveGalleryAsync(gallery)` |
| `DeleteGallery(guid, id)` | `DeleteGalleryAsync(galleryId)` |
| `GetAssetByID(mediaID, guid)` | `GetAssetAsync(mediaId)` |
| `GetAssetByURL(url, guid)` | `GetAssetByUrlAsync(url)` |

### Batches (`batchMethods` → `instance.Batches`)

| 1.x | 2.0 |
|---|---|
| `GetBatch(id, guid)` | `GetBatchAsync(batchId)` |
| `Retry(func)` | `WaitForBatchAsync(batchId)`; batch operations wait on their own |
| `contentMethods.GetBatchObject` / `pageMethods.GetBatchObject` | `GetBatchAsync(batchId)` |

### Containers (`containerMethods` → `instance.Containers`)

| 1.x | 2.0 |
|---|---|
| `GetContainerById(id, guid)` | `GetContainerAsync(containerId)` |
| `GetContainerByReferenceName(name, guid)` | `GetContainerByReferenceNameAsync(referenceName)` |
| `GetContainersByModel(modelId, guid)` | `GetContainersByModelAsync(modelId)` |
| `GetContainerSecurity(id, guid)` | `GetContainerSecurityAsync(containerId)` |
| `GetContainerList(guid)` | `GetContainerListAsync()` |
| `GetContainerListPaged(guid, ...)` | `GetContainerListPagedAsync(...)` |
| `GetNotificationList(id, guid)` | `GetNotificationsAsync(containerId)` |
| `SaveContainer(container, guid)` | `SaveContainerAsync(container)` |
| `DeleteContainer(id, guid)` | `DeleteContainerAsync(containerId)` |

### Content (`contentMethods` → `instance.Content`)

| 1.x | 2.0 |
|---|---|
| `GetContentItem(contentID, guid, locale)` | `GetContentItemAsync(locale, contentId)` |
| `GetContentItems(referenceName, guid, locale, filter, fields, sortDirection, sortField, take, skip)` | `GetContentListAsync(locale, referenceName, ContentListFilterModel, take, skip, fields: ..., sortField: ..., sortDirection: ...)` |
| `SaveContentItem(item, guid, locale)` † | `SaveContentItemAsync(locale, item)` |
| `SaveContentItems(items, guid, locale)` | `SaveContentItemsAsync(locale, items)`: `result.ItemIds`; failures throw instead of appearing as strings in the list |
| `DeleteContent(contentID, guid, locale, comments)` † | `DeleteContentItemAsync(locale, contentId, comments)` |
| `PublishContent(...)` † | `PublishContentItemAsync(locale, contentId, comments)` |
| `UnPublishContent(...)` † | `UnpublishContentItemAsync(locale, contentId, comments)` |
| `ApproveContent(...)` † | `ApproveContentItemAsync(locale, contentId, comments)` |
| `DeclineContent(...)` † | `DeclineContentItemAsync(locale, contentId, comments)` |
| `ContentRequestApproval(...)` † | `RequestApprovalContentItemAsync(locale, contentId, comments)` |

### Instance users (`instanceUserMethods` → `instance.Users`)

| 1.x | 2.0 |
|---|---|
| `GetUsers(guid)` | `GetUsersAsync()` |
| `SaveUser(email, roles, guid, first, last)` | `SaveUserAsync(email, roles, firstName, lastName)` |
| `DeleteUser(userID, guid)` | `DeleteUserAsync(userId)` |

### Models (`modelMethods` → `instance.Models`)

| 1.x | 2.0 |
|---|---|
| `GetContentModel(id, guid)` | `GetModelAsync(modelId)` |
| `GetModelByReferenceName(name, guid)` | `GetModelByReferenceNameAsync(referenceName)` |
| `GetContentModules(includeDefaults, guid, includeModules)` | `GetContentModelsAsync(includeDefaults, includeModules)` |
| `GetPageModules(guid, includeDefault)` | `GetComponentModelsAsync(includeDefault)` |
| `SaveModel(model, guid)` | `SaveModelAsync(model)` |
| `DeleteModel(id, guid)` | `DeleteModelAsync(modelId)` |

### Pages (`pageMethods` → `instance.Pages`)

| 1.x | 2.0 |
|---|---|
| `GetSiteMap(guid, locale)` | `GetSitemapAsync(locale)` |
| `GetPage(pageID, guid, locale)` | `GetPageAsync(locale, pageId)` |
| `SavePage(page, guid, locale, parentPageID, placeBeforePageItemID, pageIDInOtherLocale, otherLocale)` † | `SavePageAsync(locale, page, parentPageId, placeBeforePageId, otherLocale, pageIdInOtherLocale)`: parameters left `null` use the API's defaults |
| `DeletePage(...)` † | `DeletePageAsync(locale, pageId, comments)` |
| `PublishPage` / `UnPublishPage` / `ApprovePage` / `DeclinePage` / `PageRequestApproval` † | `PublishPageAsync` / `UnpublishPageAsync` / `ApprovePageAsync` / `DeclinePageAsync` / `RequestApprovalPageAsync`, each `(locale, pageId, comments)` |
| `GetPageTemplates(guid, locale, includeModuleZones, searchFilter)` | `GetPageTemplatesAsync(locale, includeModuleZones, searchFilter)` |
| `GetPageTemplate(guid, locale, id)` | `GetPageTemplateAsync(locale, pageTemplateId)` |
| `GetPageTemplateByName(guid, locale, name)` | `GetPageTemplateByNameAsync(locale, templateName)` |
| `GetPageItemTemplates(guid, locale, id)` | `GetPageTemplateZonesAsync(locale, pageTemplateId)` |
| `SavePageTemplate(guid, locale, template)` | `SavePageTemplateAsync(locale, template)` |
| `DeletePageTemplate(guid, locale, id)` | `DeletePageTemplateAsync(locale, pageTemplateId)` |

Delete methods that returned the API's message string now return `Task`; a failure throws.

## New in 2.0

Webhooks, locales, localization (initialize and translate), URL redirections, Personal Access Tokens, the current
user, batch actions and custom batches, batch workflow, cascade publish, history and comments, folder rename and
delete, Fetch API sync status and keys, and OAuth helpers. See [API coverage](docs/api-coverage.md).
