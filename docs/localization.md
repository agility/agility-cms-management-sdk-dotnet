# Locales and localization

## Locales

`client.Locales` manages the instance's locales.

```csharp
List<Locale> enabled = await client.Locales.GetLocalesAsync(guid);
LocalesResponse all = await client.Locales.GetAllLocalesAsync(guid);   // enabled or not
Locale fr = await client.Locales.GetLocaleAsync(guid, localeId);

var added = await client.Locales.SaveLocaleAsync(guid, new Locale { LocaleName = "French (Canada)", LocaleCode = "fr-ca" })
    ?? throw new InvalidOperationException("The API didn't return the saved locale.");
await client.Locales.EnableLocaleAsync(guid, added.LocaleID!.Value);
await client.Locales.DisableLocaleAsync(guid, added.LocaleID!.Value);

await client.Locales.SetSortOrderAsync(guid, [1, added.LocaleID!.Value, 3]);   // every locale ID, in order
```

## Copying content into other locales

`client.Localization` copies pages, content lists and content items from one locale to others, as a batch.
**Initialize** copies them as they are; **translate** also machine-translates them.

The requests take **version IDs** (`PageVersionIds`, `ContentVersionIds`), not page or content IDs. They're the
`VersionID` on an item's `Properties`, or the version in its history.

```csharp
var item = await client.Content.GetContentItemAsync(guid, "en-us", 42);

await client.Localization.TranslateContentItemsAsync(guid, new TranslateContentRequest
{
    LanguageCodeSource = "en-us",
    LanguageCodeTargets = ["fr-ca", "es-us"],
    ContentVersionIds = [item.Properties!.VersionID!.Value],
});
```

| Method | Copies |
|---|---|
| `InitializePagesAsync` / `TranslatePagesAsync` | pages (`PageVersionIds`) |
| `InitializeContentListsAsync` / `TranslateContentListsAsync` | whole lists (`ContentViewIds`, the container IDs) |
| `InitializeContentItemsAsync` / `TranslateContentItemsAsync` | content items (`ContentVersionIds`) |

Each waits for its batch by default; pass `waitForBatch: false` to return straight away.
