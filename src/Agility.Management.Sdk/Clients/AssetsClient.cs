using System.Net.Http.Headers;
using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>A file to upload with <see cref="AssetsClient.UploadAsync"/>.</summary>
/// <param name="FileName">The file name to store, e.g. <c>hero.jpg</c>.</param>
/// <param name="Content">The file's bytes. The SDK reads it but doesn't dispose it.</param>
/// <param name="ContentType">The MIME type. Defaults to <c>application/octet-stream</c>.</param>
public sealed record AssetUpload(string FileName, Stream Content, string? ContentType = null);

/// <summary>Assets (media), folders and galleries.</summary>
public sealed class AssetsClient
{
    private readonly ManagementConnection _connection;

    internal AssetsClient(ManagementConnection connection)
    {
        _connection = connection;
    }

    /// <summary>Uploads one or more files into a folder. <c>POST asset/upload</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="folderPath">The folder to upload into, e.g. <c>images/blog</c>. Use <c>""</c> or <c>"/"</c> for the root.</param>
    /// <param name="files">The files to upload.</param>
    /// <param name="galleryId">Also add the files to this gallery.</param>
    /// <param name="focalX">Focal point X coordinate for images, as the API expects it.</param>
    /// <param name="focalY">Focal point Y coordinate for images, as the API expects it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The uploaded assets.</returns>
    public Task<List<AssetMedia>> UploadAsync(string instanceGuid, string folderPath, IReadOnlyCollection<AssetUpload> files, int? galleryId = null, string? focalX = null, string? focalY = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folderPath);
        ArgumentNullException.ThrowIfNull(files);
        if (files.Count == 0) throw new ArgumentException("Pass at least one file.", nameof(files));

        HttpContent Build()
        {
            var form = new MultipartFormDataContent();
            foreach (var file in files)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(file.FileName);
                if (file.Content.CanSeek) file.Content.Position = 0;
                var part = new StreamContent(new NonDisposingStream(file.Content));
                part.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                form.Add(part, "files", file.FileName);
            }
            return form;
        }

        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"asset/upload", new Query()
                .Add("folderPath", folderPath).Add("groupingID", galleryId).Add("focalX", focalX).Add("focalY", focalY)),
            RequestKind.Write, ManagementJsonContext.Default.ListAssetMedia, Build, cancellationToken: cancellationToken);
    }

    /// <summary>Creates a folder. <c>POST asset/folder</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="originKey">The folder path to create, e.g. <c>images/blog</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMedia?> CreateFolderAsync(string instanceGuid, string originKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originKey);
        return _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"asset/folder", new Query().Add("originKey", originKey)),
            RequestKind.Write, ManagementJsonContext.Default.AssetMedia, cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a folder. <c>POST asset/folder/delete</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="originKey">The folder path.</param>
    /// <param name="mediaId">The folder's media ID, when known.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteFolderAsync(string instanceGuid, string originKey, int? mediaId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originKey);
        return _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"asset/folder/delete", new Query().Add("originKey", originKey).Add("mediaID", mediaId)),
            RequestKind.Write, cancellationToken: cancellationToken);
    }

    /// <summary>Renames a folder. <c>POST asset/folder/rename</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="folderName">The folder's current path.</param>
    /// <param name="newFolderName">The new path.</param>
    /// <param name="mediaId">The folder's media ID, when known.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task RenameFolderAsync(string instanceGuid, string folderName, string newFolderName, int? mediaId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(newFolderName);
        return _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"asset/folder/rename", new Query()
                .Add("folderName", folderName).Add("newFolderName", newFolderName).Add("mediaID", mediaId)),
            RequestKind.Write, cancellationToken: cancellationToken);
    }

    /// <summary>Deletes an asset. <c>DELETE asset/delete/{mediaID}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="mediaId">The asset's media ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteAssetAsync(string instanceGuid, int mediaId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(instanceGuid, $"asset/delete/{mediaId}"),
            RequestKind.Write, cancellationToken: cancellationToken);

    /// <summary>Moves an asset to another folder. <c>POST asset/move/{mediaID}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="mediaId">The asset's media ID.</param>
    /// <param name="newFolder">The destination folder path.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMedia?> MoveAssetAsync(string instanceGuid, int mediaId, string newFolder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newFolder);
        return _connection.SendAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"asset/move/{mediaId}", new Query().Add("newFolder", newFolder)),
            RequestKind.Write, ManagementJsonContext.Default.AssetMedia, cancellationToken: cancellationToken);
    }

    /// <summary>Lists assets, a page at a time. <c>GET asset/list</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="pageSize">Assets per page.</param>
    /// <param name="recordOffset">How many assets to skip.</param>
    /// <param name="updatedSince">Only assets changed after this time.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMediaList> GetMediaListAsync(string instanceGuid, int pageSize = 20, int recordOffset = 0, DateTime? updatedSince = null, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"asset/list", new Query()
                .Add("pageSize", pageSize).Add("recordOffset", recordOffset).Add("updatedSince", updatedSince)),
            RequestKind.Read, ManagementJsonContext.Default.AssetMediaList, cancellationToken: cancellationToken);

    /// <summary>Gets an asset by media ID. <c>GET asset/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="mediaId">The asset's media ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMedia> GetAssetAsync(string instanceGuid, int mediaId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"asset/{mediaId}"),
            RequestKind.Read, ManagementJsonContext.Default.AssetMedia, cancellationToken: cancellationToken);

    /// <summary>Gets an asset by its URL. <c>GET asset?url=</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="url">The asset's URL.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMedia> GetAssetByUrlAsync(string instanceGuid, string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"asset", new Query().Add("url", url)),
            RequestKind.Read, ManagementJsonContext.Default.AssetMedia, cancellationToken: cancellationToken);
    }

    /// <summary>Gets the instance's default asset container. <c>GET asset/container</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetContainer> GetDefaultContainerAsync(string instanceGuid, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"asset/container"),
            RequestKind.Read, ManagementJsonContext.Default.AssetContainer, cancellationToken: cancellationToken);

    /// <summary>Lists galleries. <c>GET asset/galleries</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="search">Only galleries whose name contains this text.</param>
    /// <param name="pageSize">Galleries per page.</param>
    /// <param name="rowIndex">How many galleries to skip.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetGalleries> GetGalleriesAsync(string instanceGuid, string? search = null, int? pageSize = null, int? rowIndex = null, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"asset/galleries", new Query().Add("search", search).Add("pageSize", pageSize).Add("rowIndex", rowIndex)),
            RequestKind.Read, ManagementJsonContext.Default.AssetGalleries, cancellationToken: cancellationToken);

    /// <summary>Gets a gallery by ID. <c>GET asset/gallery/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMediaGrouping> GetGalleryAsync(string instanceGuid, int galleryId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"asset/gallery/{galleryId}"),
            RequestKind.Read, ManagementJsonContext.Default.AssetMediaGrouping, cancellationToken: cancellationToken);

    /// <summary>Gets a gallery by name. <c>GET asset/gallery?galleryName=</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="galleryName">The gallery name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMediaGrouping?> GetGalleryByNameAsync(string instanceGuid, string galleryName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(galleryName);
        return _connection.SendAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"asset/gallery", new Query().Add("galleryName", galleryName)),
            RequestKind.Read, ManagementJsonContext.Default.AssetMediaGrouping, cancellationToken: cancellationToken);
    }

    /// <summary>Creates or updates a gallery. <c>POST asset/gallery</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="gallery">The gallery. Use a <c>MediaGroupingID</c> of 0 or less to create one.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AssetMediaGrouping> SaveGalleryAsync(string instanceGuid, AssetMediaGrouping gallery, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gallery);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(instanceGuid, $"asset/gallery"),
            RequestKind.Write, ManagementJsonContext.Default.AssetMediaGrouping,
            ManagementConnection.Json(gallery, ManagementJsonContext.Default.AssetMediaGrouping), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a gallery. <c>DELETE asset/gallery/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteGalleryAsync(string instanceGuid, int galleryId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(instanceGuid, $"asset/gallery/{galleryId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
}
