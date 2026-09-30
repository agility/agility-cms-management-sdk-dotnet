using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>The signed-in user.</summary>
public sealed class UsersClient
{
    private readonly ManagementConnection _connection;

    internal UsersClient(ManagementConnection connection) => _connection = connection;

    /// <summary>Gets the user the access token belongs to, with their instances. <c>GET /api/v1/users/me</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<ServerUser> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.ServerUri($"users/me"),
            RequestKind.Read, ManagementJsonContext.Default.ServerUser, cancellationToken: cancellationToken);
}

/// <summary>
/// Personal Access Tokens: long-lived tokens for scripts and CI. Managing tokens needs an OAuth access token;
/// a Personal Access Token can't create, list or revoke tokens.
/// </summary>
public sealed class PersonalAccessTokensClient
{
    private readonly ManagementConnection _connection;

    internal PersonalAccessTokensClient(ManagementConnection connection) => _connection = connection;

    /// <summary>
    /// Creates a token. <c>POST /api/v1/tokens/create</c>. The token value is in the response's <c>Token</c>
    /// and is never returned again, so store it now.
    /// </summary>
    /// <param name="request">The token's name and optional expiry date (at most two years; the default).</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PersonalAccessTokenCreationResponse> CreateTokenAsync(PersonalAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.ServerUri($"tokens/create"),
            RequestKind.Write, ManagementJsonContext.Default.PersonalAccessTokenCreationResponse,
            ManagementConnection.Json(request, ManagementJsonContext.Default.PersonalAccessTokenRequest), cancellationToken: cancellationToken);
    }

    /// <summary>Lists your tokens, without their values. <c>GET /api/v1/tokens/list</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PersonalAccessTokenListResponse> GetTokensAsync(CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.ServerUri($"tokens/list"),
            RequestKind.Read, ManagementJsonContext.Default.PersonalAccessTokenListResponse, cancellationToken: cancellationToken);

    /// <summary>Gets a token's details, without its value. <c>GET /api/v1/tokens/{tokenId}</c></summary>
    /// <param name="tokenId">The token ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PersonalAccessTokenResponse> GetTokenAsync(Guid tokenId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.ServerUri($"tokens/{tokenId}"),
            RequestKind.Read, ManagementJsonContext.Default.PersonalAccessTokenResponse, cancellationToken: cancellationToken);

    /// <summary>Renames a token, or enables or disables it. <c>PUT /api/v1/tokens/{tokenId}/update</c></summary>
    /// <param name="tokenId">The token ID.</param>
    /// <param name="request">The changes.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<PersonalAccessTokenResponse> UpdateTokenAsync(Guid tokenId, PersonalAccessTokenUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _connection.SendRequiredAsync(HttpMethod.Put, _connection.ServerUri($"tokens/{tokenId}/update"),
            RequestKind.Write, ManagementJsonContext.Default.PersonalAccessTokenResponse,
            ManagementConnection.Json(request, ManagementJsonContext.Default.PersonalAccessTokenUpdateRequest), cancellationToken: cancellationToken);
    }

    /// <summary>Revokes a token. <c>DELETE /api/v1/tokens/{tokenId}/delete</c></summary>
    /// <param name="tokenId">The token ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task RevokeTokenAsync(Guid tokenId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.ServerUri($"tokens/{tokenId}/delete"),
            RequestKind.Write, cancellationToken: cancellationToken);
}

/// <summary>The API's enum values and their names.</summary>
public sealed class TypesClient
{
    private readonly ManagementConnection _connection;

    internal TypesClient(ManagementConnection connection) => _connection = connection;

    /// <summary>Lists the API's enums (batch types, operation types, item states, ...). <c>GET /api/v1/types</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<AllTypesResponse> GetAllTypesAsync(CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.ServerUri($"types"),
            RequestKind.Read, ManagementJsonContext.Default.AllTypesResponse, anonymous: true, cancellationToken: cancellationToken);
}
