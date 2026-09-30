# Concepts

How the Management API behaves, and what the SDK does about it. Read this before your first write.

- [Clients and instances](#clients-and-instances)
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

## Clients and instances

Create one `AgilityManagementClient` and reuse it; it's thread-safe. `client.ForInstance(guid)` is cheap and
returns an `AgilityInstanceClient` for one instance. Methods take the locale first where the API needs one:

```csharp
var instance = client.ForInstance("1234abcd-u");
var page = await instance.Pages.GetPageAsync("en-us", pageId);
```

To work across several instances, call `ForInstance` for each; they share the client's connection pool.

## Batches

Saves, deletes and workflow operations (publish, unpublish, approve, decline, request approval) don't happen
during the HTTP request. The API queues a **batch** and returns its ID; the change happens when the batch is
processed, usually within seconds. Reading the item back before that shows the old value.

The SDK waits for you. Every batch-producing method has a `waitForBatch` parameter (default `true`) and returns a
`BatchResult`:

```csharp
var result = await instance.Content.SaveContentItemAsync("en-us", item);
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
| Aborted or deleted | `AgilityBatchException` |
| Not processed within `BatchPolling.Timeout` (15 min) | `AgilityBatchTimeoutException` with the batch ID and its last state. The batch keeps running on the server. |
| `cancellationToken` cancelled | `OperationCanceledException`. The batch keeps running. |

To fire and forget, or to wait later:

```csharp
var queued = await instance.Content.PublishContentItemAsync("en-us", id, waitForBatch: false);
// ... later
var batch = await instance.Batches.WaitForBatchAsync(queued.BatchId);
```

`PublishContentItemCascadeAsync` and `PublishPageCascadeAsync` can create several batches and return
`BatchCreateResult.BatchIDs`; wait for each one with `WaitForBatchAsync`.

## Saves land in Staging

Saving a published item moves it back to Staging. The live site keeps serving the previous version until you
publish again. Changing one field on a live item takes two batches:

```csharp
var item = await instance.Content.GetContentItemAsync("en-us", id);
item.Fields!["title"] = "New title";
await instance.Content.SaveContentItemAsync("en-us", item);
await instance.Content.PublishContentItemAsync("en-us", id);
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

Because the API treats a sent list as a replacement, the SDK's models leave every collection and dictionary out
of the request when it's `null`. Set a collection only when you mean to send it; set it to an empty list only when
you mean "none". Other `null` properties are sent as `null`.

## Errors

| Exception | When |
|---|---|
| `AgilityManagementException` | Any error from the API or the network. `StatusCode`, `ApiMessage` (the API's own message), `ResponseBody`, `Problem` (for RFC 7807 responses), `RequestId`, `Method` and `RequestUri` say what went wrong. Network failures and timeouts keep the original exception as `InnerException`. |
| `AgilityBatchException` | A batch was processed with failures, aborted or deleted. `BatchId` and `Batch` show what happened. |
| `AgilityBatchTimeoutException` | A batch didn't finish in time. `Waited` says how long the SDK waited. |
| `ArgumentException` | A required argument was missing, or an instance GUID has an unknown region suffix. |
| `OperationCanceledException` | Your `CancellationToken` was cancelled. Not wrapped. |

```csharp
try
{
    await instance.Content.SaveContentItemAsync("en-us", item);
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
await instance.SyncStatus.WaitForFetchApiSyncAsync(SyncMode.Fetch);
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

An unknown suffix throws an `ArgumentException` from `ForInstance`, rather than sending requests to the wrong
region. Server-level calls (`client.Users`, `client.PersonalAccessTokens`, `client.OAuth`) go to
`https://mgmt.aglty.io`. `BaseUrl` overrides all of this, for a local or test deployment of the API.

## Options reference

| Option | Default | |
|---|---|---|
| `AccessToken` | — | A PAT or OAuth access token |
| `AccessTokenProvider` | — | Supplies a token per request; takes precedence over `AccessToken` |
| `BaseUrl` | from the GUID | Override the API host |
| `ApplicationName` | — | Appended to the `User-Agent`, e.g. `my-job/1.0` |
| `BatchPolling.Interval` | 3 s | Time between batch status checks |
| `BatchPolling.Timeout` | 15 min | How long to wait for a batch |
| `BatchPolling.NotFoundGracePeriod` | 30 s | How long a new batch may 404 |
| `Retry.MaxRetries` | 3 | Retries for reads; 0 turns them off |
| `Retry.BaseDelay` | 500 ms | First retry delay, doubled each time |
| `Retry.MaxDelay` | 30 s | Longest single delay, including `Retry-After` |

Every request sends `User-Agent: agility-management-sdk-dotnet/<version> (<runtime>; <OS>)`, followed by
`ApplicationName` if you set one.
