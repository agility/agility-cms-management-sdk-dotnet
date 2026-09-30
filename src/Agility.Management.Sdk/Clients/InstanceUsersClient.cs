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
    private readonly string _guid;

    internal InstanceUsersClient(ManagementConnection connection, string guid)
    {
        _connection = connection;
        _guid = guid;
    }

    /// <summary>Lists the instance's users. <c>GET user/list</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<WebsiteUser>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"user/list"),
            RequestKind.Read, ManagementJsonContext.Default.ListWebsiteUser, cancellationToken: cancellationToken);

    /// <summary>Adds a user to the instance, or updates their roles. <c>POST user/save</c></summary>
    /// <param name="emailAddress">The user's email address.</param>
    /// <param name="roles">The roles to give the user. They replace the user's current roles.</param>
    /// <param name="firstName">The user's first name, for a new user.</param>
    /// <param name="lastName">The user's last name, for a new user.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<InstanceUser> SaveUserAsync(
        string emailAddress, IReadOnlyList<InstanceRole> roles, string? firstName = null, string? lastName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentNullException.ThrowIfNull(roles);
        var list = roles as List<InstanceRole> ?? [.. roles];
        return _connection.SendRequiredAsync(HttpMethod.Post,
            _connection.InstanceUri(_guid, $"user/save", new Query()
                .Add("emailAddress", emailAddress).Add("firstName", firstName).Add("lastName", lastName)),
            RequestKind.Write, ManagementJsonContext.Default.InstanceUser,
            ManagementConnection.Json(list, ManagementJsonContext.Default.ListInstanceRole), cancellationToken: cancellationToken);
    }

    /// <summary>Removes a user from the instance. <c>DELETE user/delete/{userID}</c></summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(_guid, $"user/delete/{userId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
}
