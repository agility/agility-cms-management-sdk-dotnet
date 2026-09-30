# URL redirections

`instance.UrlRedirections` manages redirects.

```csharp
UrlRedirectionSaveResult saved = await instance.UrlRedirections.SaveUrlRedirectionsAsync(
[
    new UrlRedirection { UrlRedirectionID = 0, OriginUrl = "/old-page", DestinationUrl = "/new-page", HttpCode = 301 },
    new UrlRedirection { UrlRedirectionID = 0, OriginUrl = "/promo", DestinationUrl = "https://example.com/sale", HttpCode = 302 },
]);

UrlRedirectionDeleteResult deleted = await instance.UrlRedirections.DeleteUrlRedirectionsAsync([12, 13]);
```

Use a `UrlRedirectionID` of `0` to create a redirection, or an existing ID to update it. The save and delete
results report each item's outcome.

## Spreadsheets

Export every redirection as an Excel file, edit it, and import it back:

```csharp
await using (var export = await instance.UrlRedirections.ExportUrlRedirectionsAsync())
await using (var file = File.Create("redirections.xlsx"))
    await export.CopyToAsync(file);

await using var edited = File.OpenRead("redirections.xlsx");
UrlRedirectionSaveResult imported = await instance.UrlRedirections.ImportUrlRedirectionsAsync(edited);
```
