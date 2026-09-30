using Agility.Management.Sdk.Clients;
using Agility.Management.Sdk.Http;

namespace Agility.Management.Sdk;

/// <summary>
/// Everything in one Agility instance. Get one from <see cref="AgilityManagementClient.ForInstance"/>.
/// It's cheap to create and safe to share across threads.
/// </summary>
public sealed class AgilityInstanceClient
{
    private readonly ManagementConnection _connection;

    internal AgilityInstanceClient(ManagementConnection connection, string instanceGuid)
    {
        _connection = connection;
        InstanceGuid = instanceGuid;
        // Resolve the region now, so a bad GUID fails here rather than on the first request.
        BaseUrl = connection.Options.BaseUrl ?? AgilityRegions.ResolveBaseUrl(instanceGuid);

        Batches = new BatchesClient(connection, instanceGuid);
        Assets = new AssetsClient(connection, instanceGuid);
        Containers = new ContainersClient(connection, instanceGuid);
        Content = new ContentClient(connection, instanceGuid, Batches);
        Pages = new PagesClient(connection, instanceGuid, Batches);
        Models = new ModelsClient(connection, instanceGuid);
        Locales = new LocalesClient(connection, instanceGuid);
        Localization = new LocalizationClient(connection, instanceGuid, Batches);
        Users = new InstanceUsersClient(connection, instanceGuid);
        UrlRedirections = new UrlRedirectionsClient(connection, instanceGuid);
        Webhooks = new WebhooksClient(connection, instanceGuid);
        SyncStatus = new SyncStatusClient(connection, instanceGuid);
    }

    /// <summary>The instance GUID.</summary>
    public string InstanceGuid { get; }

    /// <summary>The Management API host this instance's requests go to.</summary>
    public Uri BaseUrl { get; }

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

    /// <summary>The instance's users and roles.</summary>
    public InstanceUsersClient Users { get; }

    /// <summary>URL redirections.</summary>
    public UrlRedirectionsClient UrlRedirections { get; }

    /// <summary>Webhooks.</summary>
    public WebhooksClient Webhooks { get; }

    /// <summary>Fetch API sync status.</summary>
    public SyncStatusClient SyncStatus { get; }

    /// <summary>Gets the instance's Fetch API key for published content. <c>GET /oauth/getfetchkey</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<string> GetFetchApiKeyAsync(CancellationToken cancellationToken = default) => GetKeyAsync("getfetchkey", cancellationToken);

    /// <summary>Gets the instance's Fetch API key for preview (staging) content. <c>GET /oauth/getpreviewkey</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<string> GetPreviewApiKeyAsync(CancellationToken cancellationToken = default) => GetKeyAsync("getpreviewkey", cancellationToken);

    private Task<string> GetKeyAsync(string route, CancellationToken cancellationToken) =>
        _connection.SendForStringAsync(HttpMethod.Get,
            new Uri($"{BaseUrl.AbsoluteUri.TrimEnd('/')}/oauth/{route}{new Query().Add("guid", InstanceGuid)}"),
            RequestKind.Read, cancellationToken: cancellationToken);
}
