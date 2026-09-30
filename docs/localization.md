# Locales and localization

## Locales

`instance.Locales` manages the instance's locales.

```csharp
List<Locale> enabled = await instance.Locales.GetLocalesAsync();
LocalesResponse all = await instance.Locales.GetAllLocalesAsync();   // enabled or not
Locale fr = await instance.Locales.GetLocaleAsync(localeId);

var added = await instance.Locales.SaveLocaleAsync(new Locale { LocaleName = "French (Canada)", LocaleCode = "fr-ca" })
    ?? throw new InvalidOperationException("The API didn't return the saved locale.");
await instance.Locales.EnableLocaleAsync(added.LocaleID!.Value);
await instance.Locales.DisableLocaleAsync(added.LocaleID!.Value);

await instance.Locales.SetSortOrderAsync([1, added.LocaleID!.Value, 3]);   // every locale ID, in order
```

## Copying content into other locales

`instance.Localization` copies pages, content lists and content items from one locale to others, as a batch.
**Initialize** copies them as they are; **translate** also machine-translates them.

The requests take **version IDs** (`PageVersionIds`, `ContentVersionIds`), not page or content IDs. They're the
`VersionID` on an item's `Properties`, or the version in its history.

```csharp
var item = await instance.Content.GetContentItemAsync("en-us", 42);

await instance.Localization.TranslateContentItemsAsync(new TranslateContentRequest
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
