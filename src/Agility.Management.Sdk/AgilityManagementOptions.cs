namespace Agility.Management.Sdk;

/// <summary>
/// Settings for <see cref="AgilityManagementClient"/>. Every setting has a working default except the
/// credentials: set one of <see cref="AccessToken"/>, <see cref="RefreshToken"/> or <see cref="AccessTokenProvider"/>.
/// A client with no credentials can still use the OAuth sign-in endpoints and <see cref="Clients.TypesClient"/>.
/// </summary>
public sealed class AgilityManagementOptions
{
    /// <summary>
    /// A fixed bearer token: a Personal Access Token or an OAuth access token. Ignored when
    /// <see cref="AccessTokenProvider"/> is set.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// An OAuth refresh token. The client gets access tokens from it and renews them before they expire.
    /// Used when neither <see cref="AccessTokenProvider"/> nor <see cref="AccessToken"/> is set.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Called with the new refresh token when the API rotates <see cref="RefreshToken"/>, so you can store it.
    /// </summary>
    public Action<string>? RefreshTokenChanged { get; set; }

    /// <summary>
    /// Supplies a bearer token for each request. Takes precedence over <see cref="AccessToken"/> and
    /// <see cref="RefreshToken"/>.
    /// </summary>
    public IAccessTokenProvider? AccessTokenProvider { get; set; }

    /// <summary>
    /// Sends every request to this host instead of the region encoded in the instance GUID
    /// (see <see cref="AgilityRegions"/>). Use it for a local or test deployment of the API.
    /// Server-level calls (tokens, users, OAuth) also use it; they default to
    /// <see cref="AgilityRegions.DefaultServerUrl"/>.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>How long to wait for batches that saves and workflow operations create.</summary>
    public BatchPollingOptions BatchPolling { get; set; } = new();

    /// <summary>Retries for read requests that fail with a transient error.</summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Added to the <c>User-Agent</c> header after the SDK's own product token, so the API's logs can
    /// tell your application apart, e.g. <c>"my-sync-job/1.4"</c>.
    /// </summary>
    public string? ApplicationName { get; set; }

    internal void Validate()
    {
        if (BaseUrl is { IsAbsoluteUri: false })
            throw new InvalidOperationException($"{nameof(BaseUrl)} must be an absolute URI.");
        BatchPolling.Validate();
        Retry.Validate();
    }
}

/// <summary>
/// Controls how the SDK waits for a batch. Saves and workflow operations (publish, approve, and so on)
/// are asynchronous in the API: they return a batch ID and the change happens when the batch is processed.
/// </summary>
public sealed class BatchPollingOptions
{
    /// <summary>Time between status checks. Default: 3 seconds.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How long to wait for a batch to finish before throwing <see cref="AgilityBatchTimeoutException"/>.
    /// The batch keeps running on the server. Default: 15 minutes.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// A batch ID can return 404 for a moment after the API hands it out. Not-found responses are
    /// treated as "not started yet" for this long. Default: 30 seconds.
    /// </summary>
    public TimeSpan NotFoundGracePeriod { get; set; } = TimeSpan.FromSeconds(30);

    internal void Validate()
    {
        if (Interval <= TimeSpan.Zero) throw new InvalidOperationException("BatchPolling.Interval must be positive.");
        if (Timeout <= TimeSpan.Zero) throw new InvalidOperationException("BatchPolling.Timeout must be positive.");
        if (NotFoundGracePeriod < TimeSpan.Zero) throw new InvalidOperationException("BatchPolling.NotFoundGracePeriod can't be negative.");
    }
}

/// <summary>
/// Retries for read requests. Only reads are retried: a save or a workflow operation (including the
/// workflow operations the API exposes as GET, such as publish) is never retried, because repeating
/// it would repeat the change.
/// </summary>
public sealed class RetryOptions
{
    /// <summary>Retries after the first attempt. 0 turns retrying off. Default: 3.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Delay before the first retry; each later retry doubles it, plus jitter. Default: 500 ms.</summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>Upper bound for a single delay, including one asked for by a <c>Retry-After</c> header. Default: 30 seconds.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    internal void Validate()
    {
        if (MaxRetries < 0) throw new InvalidOperationException("Retry.MaxRetries can't be negative.");
        if (BaseDelay < TimeSpan.Zero || MaxDelay < TimeSpan.Zero) throw new InvalidOperationException("Retry delays can't be negative.");
    }
}
