using System.Text.Json.Nodes;
using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>Content models and component (module) models.</summary>
public sealed class ModelsClient
{
    private readonly ManagementConnection _connection;

    internal ModelsClient(ManagementConnection connection)
    {
        _connection = connection;
    }

    /// <summary>Gets a model by ID. <c>GET model/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="modelId">The model ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentModel> GetModelAsync(string instanceGuid, int modelId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"model/{modelId}"),
            RequestKind.Read, ManagementJsonContext.Default.ContentModel, cancellationToken: cancellationToken);

    /// <summary>Gets a model by reference name. <c>GET model/{referenceName}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="referenceName">The model's reference name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentModel> GetModelByReferenceNameAsync(string instanceGuid, string referenceName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenceName);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"model/{referenceName}"),
            RequestKind.Read, ManagementJsonContext.Default.ContentModel, cancellationToken: cancellationToken);
    }

    /// <summary>Lists content models. <c>GET model/list/{includeDefaults}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="includeDefaults">Include Agility's built-in models.</param>
    /// <param name="includeModules">Include component (module) models too.</param>
    /// <param name="updatedSince">Only models changed after this time.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentModel>> GetContentModelsAsync(string instanceGuid, bool includeDefaults = false, bool? includeModules = null, DateTime? updatedSince = null, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"model/list/{includeDefaults}", new Query()
                .Add("includeModules", includeModules).Add("updatedSince", updatedSince)),
            RequestKind.Read, ManagementJsonContext.Default.ListContentModel, cancellationToken: cancellationToken);

    /// <summary>Lists component (module) models. <c>GET model/list-page-modules/{includeDefault}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="includeDefault">Include Agility's built-in components.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<ContentModel>> GetComponentModelsAsync(string instanceGuid, bool includeDefault = false, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"model/list-page-modules/{includeDefault}"),
            RequestKind.Read, ManagementJsonContext.Default.ListContentModel, cancellationToken: cancellationToken);

    /// <summary>Lists the field types a model can use. <c>GET model/field-types</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<string>> GetFieldTypesAsync(string instanceGuid, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"model/field-types"),
            RequestKind.Read, ManagementJsonContext.Default.ListString, cancellationToken: cancellationToken);

    /// <summary>
    /// Lists the field types the instance's models use, including custom fields.
    /// <c>GET model/used-field-types</c>. The spec doesn't describe the response, so it's returned as JSON.
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="includeDefaults">Include Agility's built-in models.</param>
    /// <param name="includeModules">Include component (module) models.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<JsonNode?> GetUsedFieldTypesAsync(string instanceGuid, bool? includeDefaults = null, bool? includeModules = null, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"model/used-field-types", new Query()
                .Add("includeDefaults", includeDefaults).Add("includeModules", includeModules)),
            RequestKind.Read, ManagementJsonContext.Default.JsonNode, cancellationToken: cancellationToken);

    /// <summary>Creates or updates a model. <c>POST model</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="model">The model. Use an <c>Id</c> of 0 or less to create one.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ContentModel> SaveModelAsync(string instanceGuid, ContentModel model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(instanceGuid, $"model"),
            RequestKind.Write, ManagementJsonContext.Default.ContentModel,
            ManagementConnection.Json(model, ManagementJsonContext.Default.ContentModel), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a model. <c>DELETE model/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="modelId">The model ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteModelAsync(string instanceGuid, int modelId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(instanceGuid, $"model/{modelId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
}
