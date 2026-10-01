# Agility CMS Management SDK for .NET

[![NuGet](https://img.shields.io/nuget/v/Agility.Management.SDK.svg)](https://www.nuget.org/packages/Agility.Management.SDK)
[![CI](https://github.com/agility/agility-cms-management-sdk-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/agility/agility-cms-management-sdk-dotnet/actions/workflows/ci.yml)

A .NET client for the [Agility CMS](https://agilitycms.com) **Management API**: create, update, publish and
delete content, pages, page templates, models, containers, assets, locales, webhooks, URL redirections and
more. It covers every operation in the API's OpenAPI spec ([coverage](docs/api-coverage.md)).

For *reading published content* on a website, use the Fetch API and its SDKs instead; this SDK is for
managing content.

- **Target:** .NET 10
- **Upgrading from 1.x?** See [MIGRATION.md](MIGRATION.md). Version 2.0 is a new API shape.

## Install

```sh
dotnet add package Agility.Management.SDK
```

## Quick start

```csharp
using Agility.Management.Sdk;

using var client = new AgilityManagementClient(new AgilityManagementOptions
{
    AccessToken = Environment.GetEnvironmentVariable("AGILITY_TOKEN"), // a Personal Access Token
});

var guid = "1234abcd-u";   // your instance GUID: every instance-level call takes it first

// Read, change and save a content item, then publish it.
var item = await client.Content.GetContentItemAsync(guid, "en-us", 42);
item.Fields!["title"] = "Updated from .NET";

var saved = await client.Content.SaveContentItemAsync(guid, "en-us", item);      // waits for the save to finish
await client.Content.PublishContentItemAsync(guid, "en-us", saved.ItemId!.Value); // saves land in Staging
```

The client has one property per area. Instance-level methods take the instance GUID first, then the locale where
the route has one, then IDs, then optional settings:

| Property | What it covers | Guide |
|---|---|---|
| `Content` | content items: get, list and filter, save, delete, workflow, history, comments | [content](docs/content.md) |
| `Pages` | pages, sitemap, page templates and their zones | [pages](docs/pages.md) |
| `Models`, `Containers` | content and component models, containers | [models and containers](docs/models-and-containers.md) |
| `Assets` | upload, folders, galleries | [assets](docs/assets.md) |
| `Batches` | the batches behind saves and workflow | [concepts](docs/concepts.md#batches) |
| `Locales`, `Localization` | locales; copying and translating into other locales | [localization](docs/localization.md) |
| `Webhooks` | webhooks, delivery history, signing secrets | [webhooks](docs/webhooks.md) |
| `UrlRedirections` | redirections, spreadsheet import and export | [URL redirections](docs/url-redirections.md) |
| `InstanceUsers` | the instance's users and roles | [users and tokens](docs/users-and-tokens.md) |
| `SyncStatus` | whether published changes have reached the Fetch API | [concepts](docs/concepts.md#waiting-for-the-fetch-api) |

Server-level areas don't take a GUID: `client.ServerUsers` (the signed-in user), `client.PersonalAccessTokens`,
`client.OAuth` and `client.Types`. The method names and argument order line up with the TypeScript Management SDK.

## Authentication

- **Personal Access Token (PAT):** the simplest option for scripts, CI and server-side jobs. Set
  `AccessToken`. See [Personal Access Tokens](https://agilitycms.com/docs/developers/personal-access-tokens).
- **OAuth:** for applications that sign users in. Use `client.OAuth` for the sign-in flow and a
  `RefreshTokenAccessTokenProvider` to keep the access token fresh.

A few endpoints refuse PATs (instance user management and token management). [Authentication](docs/authentication.md)
covers both options, token providers, and which endpoints need OAuth.

## Dependency injection

```csharp
builder.Services.AddAgilityManagement(o =>
{
    o.AccessToken = builder.Configuration["Agility:Token"];
    o.ApplicationName = "my-sync-service/1.0";   // added to the User-Agent
});

// Then inject AgilityManagementClient.
```

`AddAgilityManagement` registers the client as a typed `HttpClient` through `IHttpClientFactory`, and returns
the `IHttpClientBuilder` so you can add handlers. You can also pass your own `HttpClient` to the constructor.

## Things the API does that you need to know

The Management API has a few behaviours that surprise people. The SDK handles what it can; the rest is in
[concepts](docs/concepts.md).

1. **Saves and workflow operations are asynchronous.** The API returns a batch ID and makes the change when
   the batch is processed. SDK methods wait for the batch by default and return a `BatchResult`; pass
   `waitForBatch: false` to get the ID straight away.
2. **A save lands in Staging**, even for a published item. Publish it afterwards for the change to go live.
3. **A save replaces the whole item.** Read it, change what you need, and send it all back.
4. **A page template's zone list is complete.** Zones you leave out are deleted. A zone's `DefaultModules`
   replaces its default components, and `[]` clears them. Leave either `null` to keep what's there. Reading a
   template and saving it back keeps everything.
5. **Workflow operations such as publish are HTTP `GET`s** that change state. The SDK never retries them, or any
   other write; it retries reads only.

## Errors and retries

- Every error from the API or the network is an `AgilityManagementException`, with the status code, the API's
  message, the response body and a request ID. Bad arguments throw `ArgumentException`, and calling an
  authenticated endpoint with no credentials throws `InvalidOperationException`.
- A batch that finishes with failed items throws `AgilityBatchException`, which carries the batch.
- A batch that doesn't finish in time throws `AgilityBatchTimeoutException`. The batch keeps running on the server.
- Reads are retried on 408, 429, 500, 502, 503 and 504 responses and network errors, with exponential backoff and
  `Retry-After` support (`Retry` in the options). Writes are never retried.
- Every method takes a `CancellationToken`.

## Regions

The instance GUID's suffix selects the API host: `-u` USA, `-us2` USA 2, `-c` Canada, `-e` Europe, `-a`
Australia, `-d` dev; a GUID with no suffix is USA. An unknown suffix throws rather than guessing. Set
`BaseUrl` to point at another host.

## Documentation

- [Concepts](docs/concepts.md): batches, staging, whole-item saves, page template zones, errors, retries
- [Authentication](docs/authentication.md)
- Guides: [content](docs/content.md) · [pages](docs/pages.md) · [models and containers](docs/models-and-containers.md) ·
  [assets](docs/assets.md) · [localization](docs/localization.md) · [webhooks](docs/webhooks.md) ·
  [URL redirections](docs/url-redirections.md) · [users and tokens](docs/users-and-tokens.md)
- [API coverage](docs/api-coverage.md): every API operation and the SDK method that calls it
- [Samples](samples/Agility.Management.Sdk.Samples): compiled examples of common tasks
- [Migrating from 1.x](MIGRATION.md) · [Changelog](CHANGELOG.md) · [Contributing](CONTRIBUTING.md)

Every public type and method has XML documentation, so IntelliSense shows the API route and behaviour.

## License

MIT. See [license.txt](license.txt).
