# Changelog

This project follows [Semantic Versioning](https://semver.org/). Breaking changes happen only in a major version.

## [2.0.0] - Unreleased

A rewrite for .NET 10. See [MIGRATION.md](MIGRATION.md) for how to upgrade.

### Added

- Coverage of every operation in the Management API's OpenAPI spec (113 of 115; the other two are browser
  redirects), checked by a test. New areas: webhooks, locales, localization (initialize and translate), URL
  redirections, Personal Access Tokens, the current user, API enums, custom batches and batch actions, batch
  workflow, cascade publish, content and page history and comments, asset folder rename and delete, Fetch API sync
  status and keys, OAuth sign-in and refresh.
- `AgilityManagementClient` with `ForInstance(guid)`, and `AddAgilityManagement` for dependency injection
  through `IHttpClientFactory`. A caller-supplied `HttpClient` is also accepted.
- `BatchResult` from every save and workflow operation, which waits for the batch by default
  (`waitForBatch: false` to skip).
- Typed errors: `AgilityManagementException`, `AgilityBatchException`, `AgilityBatchTimeoutException`.
- Token providers: `AccessToken`, `RefreshToken` (with automatic renewal), or your own `IAccessTokenProvider`.
- Retries with backoff for reads on 408, 429 and 5xx; writes are never retried.
- `CancellationToken` on every method.
- An identifying `User-Agent` (`agility-management-sdk-dotnet/<version>`), with an optional application name.
- XML documentation on the whole public API; guides in `docs/`; compiled samples.
- Source Link and symbol packages; the package is trimming- and AOT-compatible.

### Changed

- Targets .NET 10 (was .NET 6).
- Models are generated from the API's OpenAPI schemas (`tools/GenerateModels.cs`), so some are renamed
  (`Container` → `ContentContainer`, `Model` → `ContentModel`, `Media` → `AssetMedia`) and properties are PascalCase.
- Collections on models are `null` until set, and a `null` collection is left out of the request, so the API keeps
  the stored value.
- Content lists use the documented `POST {locale}/list/{referenceName}` route with a filter model.
- Uses `System.Net.Http` and source-generated `System.Text.Json`; RestSharp is no longer a dependency.

### Fixed

- `SavePageTemplateAsync` no longer clears every zone's default components.
- Instances in the USA 2 region (`-us2`) are sent to `mgmt-usa2.aglty.io`. An unknown region suffix is now an error.
- Batch waits last the full configured time (1.x stopped at half).
- Query values are URL-encoded.
- `Options.BaseUrl` is honoured.

### Removed

- `ClientInstance`, `Options` and the `*Methods` classes (replaced as above).
- `ContentItem.GetField`; read `Fields` directly.
- The `filter` string on content lists (the route 1.x used ignored it).

## 1.x

The 1.x line (.NET 6) gets fixes only. The `SavePageTemplate` default-components fix ships as a 1.0.x patch.
