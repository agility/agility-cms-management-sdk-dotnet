using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// The instance's users and their roles. These endpoints refuse Personal Access Tokens; call them with an
/// OAuth access token.
/// </summary>
public sealed class InstanceUsersClient
{
    private readonly ManagementConnection _connection;

    internal InstanceUsersClient(ManagementConnection connection)
    {
        _connection = connection;
    }

    /// <summary>Lists the instance's users. <c>GET user/list</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<WebsiteUser>> GetUsersAsync(string instanceGuid, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"user/list"),
            RequestKind.Read, ManagementJsonContext.Default.ListWebsiteUser, cancellationToken: cancellationToken);

    /// <summary>Adds a user to the instance, or updates their roles. <c>POST user/save</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="emailAddress">The user's email address.</param>
    /// <param name="roles">The roles to give the user. They replace the user's current roles.</param>
    /// <param name="firstName">The user's first name, for a new user.</param>
    /// <param name="lastName">The user's last name, for a new user.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<InstanceUser> SaveUserAsync(string instanceGuid, string emailAddress, IReadOnlyList<InstanceRole> roles, string? firstName = null, string? lastName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentNullException.ThrowIfNull(roles);
        var list = roles as List<InstanceRole> ?? [.. roles];
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(instanceGuid, $"user/save", new Query()
                .Add("emailAddress", emailAddress).Add("firstName", firstName).Add("lastName", lastName)),
            RequestKind.Write, ManagementJsonContext.Default.InstanceUser,
            ManagementConnection.Json(list, ManagementJsonContext.Default.ListInstanceRole), cancellationToken: cancellationToken);
    }

    /// <summary>Removes a user from the instance. <c>DELETE user/delete/{userID}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteUserAsync(string instanceGuid, int userId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(instanceGuid, $"user/delete/{userId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
}
