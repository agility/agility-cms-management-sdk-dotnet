# Contributing

## Setup

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). Then:

```sh
dotnet build
dotnet test
```

`dotnet test` runs the offline tests (no network, no credentials) and the live integration tests, which skip
themselves unless you configure them (below).

## Layout

| Path | |
|---|---|
| `src/Agility.Management.Sdk/` | the library |
| `src/Agility.Management.Sdk/Clients/` | one client per API area |
| `src/Agility.Management.Sdk/Models/Generated/` | models generated from the spec. **Don't edit**; add partial classes in `Models/` |
| `spec/management-api.openapi.json` | the API spec snapshot everything is generated and tested against |
| `spec/enum-names.json` | names for the spec's integer enums, which the spec doesn't carry |
| `tools/GenerateModels.cs` | the model generator |
| `tests/Agility.Management.Sdk.Tests/` | offline tests |
| `tests/Agility.Management.Sdk.IntegrationTests/` | opt-in tests against a real instance |
| `samples/` | runnable sample plus the guides' code, compiled so the docs stay correct |
| `docs/` | guides; `api-coverage.md` is generated |

## When the API changes

1. Replace `spec/management-api.openapi.json` with the new spec
   (`curl https://mgmt.aglty.io/swagger/v1/swagger.json | jq . > spec/management-api.openapi.json`). The weekly
   *API spec drift* workflow tells you when it's out of date.
2. Regenerate the models: `dotnet run tools/GenerateModels.cs`. If a new integer enum appears, add its names to
   `spec/enum-names.json` (from `management-api-dotnet`'s `Agility.Management.Domain/Enums`).
3. Run `dotnet test`. `SpecCoverageTests` fails for any operation no SDK method calls, and for any SDK call that
   isn't in the spec. Add or fix client methods until it passes.
4. Regenerate the coverage doc: `AGILITY_UPDATE_DOCS=1 dotnet test`.
5. Update the guides, the samples and `CHANGELOG.md`.

CI fails if the generated models or `docs/api-coverage.md` don't match what's committed.

## Conventions

- Every public member has XML docs that name the API route.
- Instance-level methods take the locale first (when the route has one), then IDs, then optional parameters,
  then `CancellationToken`.
- A method that only reads uses `RequestKind.Read`, so it's retried; anything that changes state uses
  `RequestKind.Write`, even when the API exposes it as `GET`.
- Batch-producing methods take `waitForBatch` and return `BatchResult`.
- Collections on models stay `null` unless set: the API reads a sent list as a replacement.

## Integration tests

They run against a **test instance** and skip themselves without credentials. Put the credentials in
`~/.config/agility/sdk-test.env` (`chmod 600`):

```sh
export AGILITY_MGMT_TOKEN=<personal access token>
export AGILITY_INSTANCE_GUID=<instance guid>
export AGILITY_LOCALE=en-us            # optional
export AGILITY_ALLOW_WRITES=true       # optional: also create, change and delete things
```

then run `tools/run-integration-tests.sh`. The write tests clean up after themselves, but only point them at an
instance you can afford to change.

In CI, the *Integration tests* workflow runs the same suite nightly, on pushes to `main`, on pull requests from
branches in this repository (not forks), and on demand, using the
`AGILITY_MGMT_TOKEN` and `AGILITY_INSTANCE_GUID` secrets of the repository's `qa` environment (secrets, so
GitHub masks them in the public logs).

## Releasing

1. Set `<Version>` in `src/Agility.Management.Sdk/Agility.Management.Sdk.csproj` and move the `CHANGELOG.md`
   entries under that version.
2. Merge to `main`, then tag: `git tag v2.0.0 && git push origin v2.0.0`.
3. The *Release* workflow checks the tag matches the version, builds, tests, packs and publishes to NuGet with
   Trusted Publishing.

## This repository is public

Don't put customer names, instance GUIDs, tokens or internal details in code, tests, commits or pull requests.
