# Concepts

How the Management API behaves, and what the SDK does about it. Read this before your first write.

- [The client and instance GUIDs](#the-client-and-instance-guids)
- [Batches](#batches)
- [Saves land in Staging](#saves-land-in-staging)
- [Saves replace the whole item](#saves-replace-the-whole-item)
- [Page templates: zones and default components](#page-templates-zones-and-default-components)
- [Unset, null and empty collections](#unset-null-and-empty-collections)
- [Errors](#errors)
- [Retries](#retries)
- [Waiting for the Fetch API](#waiting-for-the-fetch-api)
- [Regions](#regions)
- [Options reference](#options-reference)

## The client and instance GUIDs

Create one `AgilityManagementClient` and reuse it; it's thread-safe. Every instance-level method takes the
instance GUID first, then the locale where the route has one, then IDs, then optional settings:

```csharp
var page = await client.Pages.GetPageAsync("1234abcd-u", "en-us", pageId);
```

One client works with any number of instances: pass a different GUID. Methods with many optional settings take an
options object (`ContentListOptions`, `SavePageOptions`, `ContainerListOptions`); the rest take named optional
parameters.

## Batches

Saves, deletes and workflow operations (publish, unpublish, approve, decline, request approval) don't happen
during the HTTP request. The API queues a **batch** and returns its ID; the change happens when the batch is
processed, usually within seconds. Reading the item back before that shows the old value.

The SDK waits for you. Every batch-producing method has a `waitForBatch` parameter (default `true`) and returns a
`BatchResult`:

```csharp
var result = await client.Content.SaveContentItemAsync(guid, "en-us", item);
result.BatchId;   // the batch
result.ItemId;    // the saved item's content ID (new items get one here)
result.ItemIds;   // every item's ID, for multi-item saves
result.Batch;     // the processed batch, with its items
```

While waiting, the SDK polls `GET batch/{id}` every `BatchPolling.Interval` (3 s). It treats a 404 in the first
`BatchPolling.NotFoundGracePeriod` (30 s) as "not created yet", because a new batch ID can briefly 404.

| Outcome | What you get |
|---|---|
| Processed, every item succeeded | `BatchResult` |
| Processed, some items failed | `AgilityBatchException` with the batch, so you can see which items succeeded. The message includes the first few item errors. |
| Aborted (at any point) or deleted | `AgilityBatchException`, as soon as the SDK sees it |
| Not processed within `BatchPolling.Timeout` (15 min) | `AgilityBatchTimeoutException` with the batch ID and its last state. The batch keeps running on the server. |
| `cancellationToken` cancelled | `OperationCanceledException`. The batch keeps running. |

To fire and forget, or to wait later:

```csharp
var queued = await client.Content.PublishContentItemAsync(guid, "en-us", id, waitForBatch: false);
// ... later
var batch = await client.Batches.WaitForBatchAsync(guid, queued.BatchId);
```

`PublishContentItemCascadeAsync` and `PublishPageCascadeAsync` can create several batches and return
`BatchCreateResult.BatchIDs`; wait for each one with `WaitForBatchAsync`.

## Saves land in Staging

Saving a published item moves it back to Staging. The live site keeps serving the previous version until you
publish again. Changing one field on a live item takes two batches:

```csharp
var item = await client.Content.GetContentItemAsync(guid, "en-us", id);
item.Fields!["title"] = "New title";
await client.Content.SaveContentItemAsync(guid, "en-us", item);
await client.Content.PublishContentItemAsync(guid, "en-us", id);
```

## Saves replace the whole item

`SaveContentItemAsync` and `SavePageAsync` replace the stored item with what you send. If you send an item with
only the field you changed, the other fields are lost. Always read the item, change it, and send the whole thing
back. `ContentItem.Fields` is a `JsonObject`, so fields the SDK doesn't know about survive the round trip
unchanged.

## Page templates: zones and default components

A page template (page model) has zones (`ContentSectionDefinitions`), and each zone can have default components
(`DefaultModules`). `SavePageTemplateAsync` reads them like this:

| You send | The API |
|---|---|
| `ContentSectionDefinitions = null` | keeps the zones as they are |
| a zone list | makes it the **complete** zone list: zones you leave out are deleted |
| a zone with `DefaultModules = null` | keeps that zone's default components |
| a zone with a `DefaultModules` list | replaces that zone's default components |
| a zone with `DefaultModules = []` | clears that zone's default components |

⚠️ **Deleting a zone hides its components on every page that uses the template.** The content survives, and
re-adding a zone with the same reference name through the API restores it and its placements. But don't send a
partial zone list by accident.

Reading a template with `GetPageTemplateAsync` and saving it back keeps everything. Use `GetPageTemplateAsync`
rather than `GetPageTemplateZonesAsync` for that: the zones route always returns `DefaultModules` as empty.

> SDK 1.x sent every zone's default components as `[]`, which cleared them on every save. 2.0 omits the list
> unless you set it.

## Unset, null and empty collections

The SDK leaves every `null` property out of the request. The API binds a missing property to its default, and:

- it treats a sent list as a replacement, so an unset collection must not go out as `null` or `[]`. Set a
  collection only when you mean to send it, and to an empty list only when you mean "none";
- it rejects an explicit `null` for many properties the spec calls nullable, with "The X field is required".

Values inside `ContentItem.Fields` are data, so a field set to `null` is still sent as `null`.

## Errors

| Exception | When |
|---|---|
| `AgilityManagementException` | Any error from the API or the network. `StatusCode`, `ApiMessage` (the API's own message), `ResponseBody`, `Problem` (for RFC 7807 responses), `RequestId`, `Method` and `RequestUri` say what went wrong. Network failures and timeouts keep the original exception as `InnerException`. |
| `AgilityBatchException` | A batch was processed with failures, aborted or deleted. `BatchId` and `Batch` show what happened. |
| `AgilityBatchTimeoutException` | A batch didn't finish in time. `Waited` says how long the SDK waited. |
| `ArgumentException` | A required argument was missing or invalid (an empty ID list, a `.` or `..` path value, a batch request with no operation), or an instance GUID has an unknown region suffix. |
| `InvalidOperationException` | An authenticated endpoint was called on a client with no credentials, or the options are invalid. |
| `TimeoutException` | `WaitForFetchApiSyncAsync` gave up. |
| `OperationCanceledException` | Your `CancellationToken` was cancelled. Not wrapped. |

```csharp
try
{
    await client.Content.SaveContentItemAsync(guid, "en-us", item);
}
catch (AgilityBatchException ex)
{
    foreach (var failed in ex.Batch?.Items?.Where(i => i.ErrorMessage is not null) ?? [])
        Console.WriteLine($"{failed.ItemID}: {failed.ErrorMessage}");
}
catch (AgilityManagementException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine(ex.ApiMessage);
}
```

## Retries

The SDK retries **reads** that fail with 408, 429, 500, 502, 503 or 504, or with a network error or timeout. It
waits `Retry.BaseDelay` (500 ms), doubling each time with jitter, up to `Retry.MaxRetries` (3) retries, and honours
`Retry-After` up to `Retry.MaxDelay` (30 s).

It **never retries a write**, because repeating a save or a publish would repeat the change. That includes the
workflow operations the API exposes as `GET` (`.../publish`, `.../approve`, ...), and content list queries are the
one `POST` that is retried, because they only read. Set `Retry.MaxRetries = 0` to turn retrying off.

## Waiting for the Fetch API

A processed publish batch means the Management API has published the item. The Fetch API (what your website reads)
syncs shortly after. To wait for that too:

```csharp
await client.SyncStatus.WaitForFetchApiSyncAsync(guid, SyncMode.Fetch);
```

## Regions

The instance GUID's suffix selects the API host:

| Suffix | Host |
|---|---|
| none, `-u` | `https://mgmt.aglty.io` |
| `-us2` | `https://mgmt-usa2.aglty.io` |
| `-c` | `https://mgmt-ca.aglty.io` |
| `-e` | `https://mgmt-eu.aglty.io` |
| `-a` | `https://mgmt-aus.aglty.io` |
| `-d` | `https://mgmt-dev.aglty.io` |

An unknown suffix throws an `ArgumentException` before any request is sent, rather than sending requests to the
wrong region. Server-level calls (`client.ServerUsers`, `client.PersonalAccessTokens`, `client.Types`, and the OAuth
sign-in and token calls on `client.OAuth`) go to `https://mgmt.aglty.io`. `client.OAuth`'s Fetch and preview API key
calls go to the instance's region. `BaseUrl` overrides all of this, for a local or test deployment of the API.

## Options reference

| Option | Default | |
|---|---|---|
| `AccessToken` | — | A PAT or OAuth access token |
| `RefreshToken` | — | An OAuth refresh token; the client gets and renews access tokens from it |
| `RefreshTokenChanged` | — | Called with the new refresh token when the API rotates it |
| `AccessTokenProvider` | — | Supplies a token per request; takes precedence over `AccessToken` and `RefreshToken` |
| `BaseUrl` | from the GUID | Override the API host |
| `ApplicationName` | — | Appended to the `User-Agent`, e.g. `my-job/1.0` |
| `BatchPolling.Interval` | 3 s | Time between batch status checks |
| `BatchPolling.Timeout` | 15 min | How long to wait for a batch |
| `BatchPolling.NotFoundGracePeriod` | 30 s | How long a new batch may 404 |
| `Retry.MaxRetries` | 3 | Retries for reads; 0 turns them off |
| `Retry.BaseDelay` | 500 ms | First retry delay, doubled each time |
| `Retry.MaxDelay` | 30 s | Longest single delay, including `Retry-After` |

Every request sends `User-Agent: agility-management-sdk-dotnet/<version> (<runtime>; <OS>)`, followed by
`ApplicationName` if you set one, and `X-Agility-SDK: agility-management-sdk-dotnet/<version>`, the header every
Agility SDK sends, because browsers don't let JavaScript set `User-Agent`.
