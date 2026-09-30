# Assets

`instance.Assets` works with media files, folders and galleries.

## Upload

```csharp
await using var file = File.OpenRead("hero.jpg");
List<AssetMedia> uploaded = await instance.Assets.UploadAsync("images/blog",
    [new AssetUpload("hero.jpg", file, "image/jpeg")]);

Console.WriteLine(uploaded[0].EdgeUrl);
```

Upload several files at once by passing more `AssetUpload`s. `galleryId` also adds them to a gallery. The SDK reads
your streams but doesn't dispose them.

## Find assets

```csharp
AssetMediaList page = await instance.Assets.GetMediaListAsync(pageSize: 100, recordOffset: 0);
AssetMedia asset = await instance.Assets.GetAssetAsync(mediaId);
AssetMedia byUrl = await instance.Assets.GetAssetByUrlAsync("https://cdn.aglty.io/abc/images/blog/hero.jpg");
AssetContainer container = await instance.Assets.GetDefaultContainerAsync();   // the CDN container and its URLs
```

`GetMediaListAsync` takes `updatedSince` to get only recently changed assets.

## Folders

```csharp
await instance.Assets.CreateFolderAsync("images/blog/2026");
await instance.Assets.RenameFolderAsync("images/blog/2026", "images/blog/archive-2026");
await instance.Assets.DeleteFolderAsync("images/blog/archive-2026");
await instance.Assets.MoveAssetAsync(mediaId, "images/blog/archive-2026");
await instance.Assets.DeleteAssetAsync(mediaId);
```

## Galleries

```csharp
AssetGalleries galleries = await instance.Assets.GetGalleriesAsync(search: "team");
AssetMediaGrouping gallery = await instance.Assets.GetGalleryAsync(galleryId);
AssetMediaGrouping? byName = await instance.Assets.GetGalleryByNameAsync("Team photos");

var created = await instance.Assets.SaveGalleryAsync(new AssetMediaGrouping
{
    MediaGroupingID = -1,
    Name = "Team photos",
    GroupingTypeID = 1,
});

await instance.Assets.DeleteGalleryAsync(created.MediaGroupingID!.Value);
```
