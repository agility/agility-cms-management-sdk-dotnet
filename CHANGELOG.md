# Changelog

This project follows [Semantic Versioning](https://semver.org/). Breaking changes happen only in a major version.

## [2.0.0] - 2026-10-01

A rewrite for .NET 10. See [MIGRATION.md](MIGRATION.md) for how to upgrade.

### Added

- Coverage of every operation in the Management API's OpenAPI spec (113 of 115; the other two are browser
  redirects), checked by a test. New areas: webhooks, locales, localization (initialize and translate), URL
  redirections, Personal Access Tokens, the current user, API enums, custom batches and batch actions, batch
  workflow, cascade publish, content and page history and comments, asset folder rename and delete, Fetch API sync
  status and keys, OAuth sign-in and refresh.
- `AgilityManagementClient` with one property per area (`Content`, `Pages`, ...), whose methods take the instance
  GUID, then the locale, then IDs, matching the TypeScript SDK. `AddAgilityManagement` registers it for dependency
  injection through `IHttpClientFactory`; a caller-supplied `HttpClient` is also accepted.
- `BatchResult` from every save and workflow operation, which waits for the batch by default
  (`waitForBatch: false` to skip).
- Typed errors: `AgilityManagementException`, `AgilityBatchException`, `AgilityBatchTimeoutException`.
- Token providers: `AccessToken`, `RefreshToken` (with automatic renewal), or your own `IAccessTokenProvider`.
- Retries with backoff for reads on 408, 429, 500, 502, 503 and 504, and network errors; writes are never retried.
- `CancellationToken` on every method.
- An identifying `User-Agent` (`agility-management-sdk-dotnet/<version>`), with an optional application name, and
  an `X-Agility-SDK` header with the same product token.
- XML documentation on the whole public API; guides in `docs/`; compiled samples.
- Source Link and symbol packages; the package is trimming- and AOT-compatible.

### Changed

- Targets .NET 10 (was .NET 6).
- Models are generated from the API's OpenAPI schemas (`tools/GenerateModels.cs`), so some are renamed
  (`Container` → `ContentContainer`, `Model` → `ContentModel`, `Media` → `AssetMedia`) and properties are PascalCase.
- Collections on models are `null` until set, and `null` properties are left out of requests: the API keeps a
  stored list it isn't sent, and rejects an explicit `null` for many properties.
- Content lists use the documented `POST {locale}/list/{referenceName}` route with a filter model.
- Uses `System.Net.Http` and source-generated `System.Text.Json`; RestSharp is no longer a dependency.

### Fixed

- `SavePageTemplateAsync` no longer clears every zone's default components.
- Instances in the USA 2 region (`-us2`) are sent to `mgmt-usa2.aglty.io`. An unknown region suffix is now an error.
- Batch waits last the full configured time (1.x stopped at half), and an aborted batch is reported as a failure.
- Query values are URL-encoded.
- `Options.BaseUrl` is honoured.

### Removed

- `ClientInstance`, `Options` and the `*Methods` classes (replaced as above).
- `ContentItem.GetField`; read `Fields` directly.
- The `filter` string on content lists (the route 1.x used ignored it).

## 1.x

The 1.x line (.NET 6) gets fixes only, from the `release/1.x` branch.

### [1.0.12-beta] - 2026-09-30

- Fixed: `SavePageTemplate` no longer clears every zone's default components. `DefaultModules` and `SharedModules` on
  `ContentSectionDefinition` are left out of the request when they're `null`, so the API keeps the zone's defaults.

[2.0.0]: https://github.com/agility/agility-cms-management-sdk-dotnet/releases/tag/v2.0.0
[1.0.12-beta]: https://github.com/agility/agility-cms-management-sdk-dotnet/releases/tag/v1.0.12-beta
