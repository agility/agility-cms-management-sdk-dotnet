using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>Containers (content lists and single-item containers).</summary>
public sealed class ContainersClient
{
    private readonly ManagementConnection _connection;

    internal ContainersClient(ManagementConnection connection)
    {
        _connection = connection;
    }

    /// <summary>Gets a container by ID. <c>GET container/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> GetContainerAsync(string instanceGuid, int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"container/{containerId}"),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainer, cancellationToken: cancellationToken);

    /// <summary>Gets a container by reference name. <c>GET container/{referenceName}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="referenceName">The container's reference name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> GetContainerByReferenceNameAsync(string instanceGuid, string referenceName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceName);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"container/{referenceName}"),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainer, cancellationToken: cancellationToken);
    }

    /// <summary>Lists the containers that use a model. <c>GET container/model/{modelId}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="modelId">The content model ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentContainer>> GetContainersByModelAsync(string instanceGuid, int modelId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"container/model/{modelId}"),
            RequestKind.Read, ManagementJsonContext.Default.ListContentContainer, cancellationToken: cancellationToken);

    /// <summary>Gets a container with its security settings. <c>GET container/{id}/security</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> GetContainerSecurityAsync(string instanceGuid, int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"container/{containerId}/security"),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainer, cancellationToken: cancellationToken);

    /// <summary>Lists every container. <c>GET container/list</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="updatedSince">Only containers changed after this time.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentContainer>> GetContainerListAsync(string instanceGuid, DateTime? updatedSince = null, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"container/list", new Query().Add("updatedSince", updatedSince)),
            RequestKind.Read, ManagementJsonContext.Default.ListContentContainer, cancellationToken: cancellationToken);

    /// <summary>Lists containers a page at a time. <c>GET container/list/paged</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="options">Paging and filtering. <see langword="null"/>: the API's defaults.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainerPagedResult> GetContainerListPagedAsync(
        string instanceGuid, ContainerListOptions? options = null, CancellationToken cancellationToken = default)
    {
        var o = options ?? new ContainerListOptions();
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"container/list/paged", new Query()
                .Add("pageSize", o.PageSize).Add("recordOffset", o.RecordOffset).Add("contentType", (int?)o.ContentType)
                .Add("includeModules", o.IncludeModules).Add("updatedSince", o.UpdatedSince)),
            RequestKind.Read, ManagementJsonContext.Default.ContentContainerPagedResult, cancellationToken: cancellationToken);
    }

    /// <summary>Lists a container's notification settings. <c>GET container/{id}/notifications</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<Notification>> GetNotificationsAsync(string instanceGuid, int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"container/{containerId}/notifications"),
            RequestKind.Read, ManagementJsonContext.Default.ListNotification, cancellationToken: cancellationToken);

    /// <summary>Creates or updates a container. <c>POST container</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="container">The container. Use a <c>ContentViewID</c> of 0 or less to create one.</param>
    /// <param name="forceReferenceName">Keep the reference name exactly as given instead of letting the API adjust it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentContainer> SaveContainerAsync(string instanceGuid, ContentContainer container, bool? forceReferenceName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"container", new Query().Add("forceReferenceName", forceReferenceName)),
            RequestKind.Write, ManagementJsonContext.Default.ContentContainer,
            ManagementConnection.Json(container, ManagementJsonContext.Default.ContentContainer), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a container. <c>DELETE container/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="containerId">The container ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteContainerAsync(string instanceGuid, int containerId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(instanceGuid, $"container/{containerId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
}
