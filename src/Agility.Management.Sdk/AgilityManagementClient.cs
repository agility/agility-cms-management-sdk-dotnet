using Agility.Management.Sdk.Clients;
using Agility.Management.Sdk.Http;

namespace Agility.Management.Sdk;

/// <summary>
/// The entry point to the Agility CMS Management API.
/// </summary>
/// <example>
/// <code>
/// var client = new AgilityManagementClient(new AgilityManagementOptions { AccessToken = pat });
/// var item = await client.Content.GetContentItemAsync("1234abcd-u", "en-us", 42);
/// </code>
/// </example>
/// <remarks>
/// Every instance-level method takes the instance GUID first, then the locale where the route has one, then IDs.
/// Create one client and reuse it for any number of instances. It's safe to share across threads. With dependency injection, use
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
        ServerUsers = new ServerUsersClient(_connection);
        PersonalAccessTokens = new PersonalAccessTokensClient(_connection);
        Types = new TypesClient(_connection);

        Batches = new BatchesClient(_connection);
        Assets = new AssetsClient(_connection);
        Containers = new ContainersClient(_connection);
        Content = new ContentClient(_connection, Batches);
        Pages = new PagesClient(_connection, Batches);
        Models = new ModelsClient(_connection);
        Locales = new LocalesClient(_connection);
        Localization = new LocalizationClient(_connection, Batches);
        InstanceUsers = new InstanceUsersClient(_connection);
        UrlRedirections = new UrlRedirectionsClient(_connection);
        Webhooks = new WebhooksClient(_connection);
        SyncStatus = new SyncStatusClient(_connection);
    }

    /// <summary>Assets (media), folders and galleries.</summary>
    public AssetsClient Assets { get; }

    /// <summary>Batches, and waiting for them.</summary>
    public BatchesClient Batches { get; }

    /// <summary>Containers.</summary>
    public ContainersClient Containers { get; }

    /// <summary>Content items.</summary>
    public ContentClient Content { get; }

    /// <summary>Pages, the sitemap and page templates.</summary>
    public PagesClient Pages { get; }

    /// <summary>Content and component models.</summary>
    public ModelsClient Models { get; }

    /// <summary>Locales.</summary>
    public LocalesClient Locales { get; }

    /// <summary>Copying and translating pages and content into other locales.</summary>
    public LocalizationClient Localization { get; }

    /// <summary>An instance's users and roles.</summary>
    public InstanceUsersClient InstanceUsers { get; }

    /// <summary>URL redirections.</summary>
    public UrlRedirectionsClient UrlRedirections { get; }

    /// <summary>Webhooks.</summary>
    public WebhooksClient Webhooks { get; }

    /// <summary>Fetch API sync status.</summary>
    public SyncStatusClient SyncStatus { get; }

    /// <summary>OAuth sign-in and token refresh, and an instance's Fetch API keys.</summary>
    public OAuthClient OAuth { get; }

    /// <summary>The signed-in user.</summary>
    public ServerUsersClient ServerUsers { get; }

    /// <summary>Personal Access Tokens.</summary>
    public PersonalAccessTokensClient PersonalAccessTokens { get; }

    /// <summary>The API's enum values.</summary>
    public TypesClient Types { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _ownedTokenProvider?.Dispose();
        _ownedHttpClient?.Dispose();
    }
}
