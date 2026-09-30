# Assets

`client.Assets` works with media files, folders and galleries.

## Upload

```csharp
await using var file = File.OpenRead("hero.jpg");
List<AssetMedia> uploaded = await client.Assets.UploadAsync(guid, "images/blog",
    [new AssetUpload("hero.jpg", file, "image/jpeg")]);

Console.WriteLine(uploaded[0].EdgeUrl);
```

Upload several files at once by passing more `AssetUpload`s. `galleryId` also adds them to a gallery. The SDK reads
your streams but doesn't dispose them.

## Find assets

```csharp
AssetMediaList page = await client.Assets.GetMediaListAsync(guid, pageSize: 100, recordOffset: 0);
AssetMedia asset = await client.Assets.GetAssetAsync(guid, mediaId);
AssetMedia byUrl = await client.Assets.GetAssetByUrlAsync(guid, "https://cdn.aglty.io/abc/images/blog/hero.jpg");
AssetContainer container = await client.Assets.GetDefaultContainerAsync(guid);   // the CDN container and its URLs
```

`GetMediaListAsync` takes `updatedSince` to get only recently changed assets.

## Folders

```csharp
await client.Assets.CreateFolderAsync(guid, "images/blog/2026");
await client.Assets.RenameFolderAsync(guid, "images/blog/2026", "images/blog/archive-2026");
await client.Assets.DeleteFolderAsync(guid, "images/blog/archive-2026");
await client.Assets.MoveAssetAsync(guid, mediaId, "images/blog/archive-2026");
await client.Assets.DeleteAssetAsync(guid, mediaId);
```

## Galleries

```csharp
AssetGalleries galleries = await client.Assets.GetGalleriesAsync(guid, search: "team");
AssetMediaGrouping gallery = await client.Assets.GetGalleryAsync(guid, galleryId);
AssetMediaGrouping? byName = await client.Assets.GetGalleryByNameAsync(guid, "Team photos");

var created = await client.Assets.SaveGalleryAsync(guid, new AssetMediaGrouping
{
    MediaGroupingID = -1,
    Name = "Team photos",
    GroupingTypeID = 1,
});

await client.Assets.DeleteGalleryAsync(guid, created.MediaGroupingID!.Value);
```
