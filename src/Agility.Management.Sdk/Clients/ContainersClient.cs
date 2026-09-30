using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>Containers (content lists and single-item containers).</summary>
public sealed class ContainersClient
{
    private readonly ManagementConnection _connection;
    private readonly string _guid;

    internal ContainersClient(ManagementConnection connection, string guid)
    {
        _connection = connection;
        _guid = guid;
    }

    /// <summary>Gets a container by ID. <c>GET container/{id}</c></summary>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> GetContainerAsync(int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"container/{containerId}"),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainer, cancellationToken: cancellationToken);

    /// <summary>Gets a container by reference name. <c>GET container/{referenceName}</c></summary>
    /// <param name="referenceName">The container's reference name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> GetContainerByReferenceNameAsync(string referenceName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceName);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"container/{referenceName}"),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainer, cancellationToken: cancellationToken);
    }

    /// <summary>Lists the containers that use a model. <c>GET container/model/{modelId}</c></summary>
    /// <param name="modelId">The content model ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentContainer>> GetContainersByModelAsync(int modelId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"container/model/{modelId}"),
            RequestKind.Read, ManagementJsonContext.Default.ListContentContainer, cancellationToken: cancellationToken);

    /// <summary>Gets a container with its security settings. <c>GET container/{id}/security</c></summary>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> GetContainerSecurityAsync(int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"container/{containerId}/security"),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainer, cancellationToken: cancellationToken);

    /// <summary>Lists every container. <c>GET container/list</c></summary>
    /// <param name="updatedSince">Only containers changed after this time.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentContainer>> GetContainerListAsync(DateTime? updatedSince = null, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"container/list", new Query().Add("updatedSince", updatedSince)),
            RequestKind.Read, ManagementJsonContext.Default.ListContentContainer, cancellationToken: cancellationToken);

    /// <summary>Lists containers a page at a time. <c>GET container/list/paged</c></summary>
    /// <param name="pageSize">Containers per page.</param>
    /// <param name="recordOffset">How many containers to skip.</param>
    /// <param name="contentType">Which kind of container to list.</param>
    /// <param name="includeModules">Include component (module) containers.</param>
    /// <param name="updatedSince">Only containers changed after this time.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainerPagedResult> GetContainerListPagedAsync(
        int? pageSize = null, int? recordOffset = null, ContentViewType? contentType = null, bool? includeModules = null,
        DateTime? updatedSince = null, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(_guid, $"container/list/paged", new Query()
                .Add("pageSize", pageSize).Add("recordOffset", recordOffset).Add("contentType", (int?)contentType)
                .Add("includeModules", includeModules).Add("updatedSince", updatedSince)),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainerPagedResult, cancellationToken: cancellationToken);

    /// <summary>Lists a container's notification settings. <c>GET container/{id}/notifications</c></summary>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<Notification>> GetNotificationsAsync(int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"container/{containerId}/notifications"),
            RequestKind.Read, ManagementJsonContext.Default.ListNotification, cancellationToken: cancellationToken);

    /// <summary>Creates or updates a container. <c>POST container</c></summary>
    /// <param name="container">The container. Use a <c>ContentViewID</c> of 0 or less to create one.</param>
    /// <param name="forceReferenceName">Keep the reference name exactly as given instead of letting the API adjust it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> SaveContainerAsync(ContentContainer container, bool? forceReferenceName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"container", new Query().Add("forceReferenceName", forceReferenceName)),
            RequestKind.Write, ManagementJsonContext.Default.ContentContainer,
            ManagementConnection.Json(container, ManagementJsonContext.Default.ContentContainer), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a container. <c>DELETE container/{id}</c></summary>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteContainerAsync(int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(_guid, $"container/{containerId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
}
