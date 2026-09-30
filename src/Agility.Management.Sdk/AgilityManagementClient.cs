using Agility.Management.Sdk.Clients;
using Agility.Management.Sdk.Http;

namespace Agility.Management.Sdk;

/// <summary>
/// The entry point to the Agility CMS Management API.
/// </summary>
/// <example>
/// <code>
/// var client = new AgilityManagementClient(new AgilityManagementOptions { AccessToken = pat });
/// var instance = client.ForInstance("1234abcd-u");
/// var item = await instance.Content.GetContentItemAsync("en-us", 42);
/// </code>
/// </example>
/// <remarks>
/// Create one client and reuse it. It's safe to share across threads. With dependency injection, use
/// <c>services.AddAgilityManagement(...)</c> instead of constructing it.
/// </remarks>
public sealed class AgilityManagementClient : IDisposable
{
    private readonly HttpClient? _ownedHttpClient;
    private readonly ManagementConnection _connection;
    private readonly RefreshTokenAccessTokenProvider? _ownedTokenProvider;

    /// <summary>Creates a client with its own <see cref="HttpClient"/>, disposed with the client.</summary>
    /// <param name="options">Credentials and settings.</param>
    public AgilityManagementClient(AgilityManagementOptions options)
        : this(options, new HttpClient(), ownsHttpClient: true) { }

    /// <summary>
    /// Creates a client that sends requests through <paramref name="httpClient"/>, which you own and dispose.
    /// Use it to share a handler, add a proxy, or plug in <c>IHttpClientFactory</c>.
    /// </summary>
    /// <param name="options">Credentials and settings.</param>
    /// <param name="httpClient">The HTTP client. Its <c>BaseAddress</c> is ignored.</param>
    public AgilityManagementClient(AgilityManagementOptions options, HttpClient httpClient)
        : this(options, httpClient, ownsHttpClient: false) { }

    internal AgilityManagementClient(AgilityManagementOptions options, HttpClient httpClient, bool ownsHttpClient, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        _connection = new ManagementConnection(httpClient, options, timeProvider);
        _ownedHttpClient = ownsHttpClient ? httpClient : null;
        OAuth = new OAuthClient(_connection);
        if (options.AccessTokenProvider is null && string.IsNullOrWhiteSpace(options.AccessToken) && !string.IsNullOrWhiteSpace(options.RefreshToken))
        {
            _ownedTokenProvider = new RefreshTokenAccessTokenProvider(OAuth, options.RefreshToken, timeProvider);
            if (options.RefreshTokenChanged is { } changed) _ownedTokenProvider.RefreshTokenChanged += (_, token) => changed(token);
            _connection.UseTokenProvider(_ownedTokenProvider);
        }
        Users = new UsersClient(_connection);
        PersonalAccessTokens = new PersonalAccessTokensClient(_connection);
        Types = new TypesClient(_connection);
    }

    /// <summary>OAuth sign-in and token refresh.</summary>
    public OAuthClient OAuth { get; }

    /// <summary>The signed-in user.</summary>
    public UsersClient Users { get; }

    /// <summary>Personal Access Tokens.</summary>
    public PersonalAccessTokensClient PersonalAccessTokens { get; }

    /// <summary>The API's enum values.</summary>
    public TypesClient Types { get; }

    /// <summary>Returns a client for one instance.</summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <exception cref="ArgumentException">The GUID's region suffix isn't known and no base URL is configured.</exception>
    public AgilityInstanceClient ForInstance(string instanceGuid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceGuid);
        return new AgilityInstanceClient(_connection, instanceGuid.Trim());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _ownedTokenProvider?.Dispose();
        _ownedHttpClient?.Dispose();
    }
}
