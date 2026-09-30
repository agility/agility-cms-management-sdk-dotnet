using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Http;

/// <summary>Whether a request may be repeated after a transient failure.</summary>
internal enum RequestKind
{
    /// <summary>Reads nothing but data. Retried on 408, 429, 5xx and network errors.</summary>
    Read,

    /// <summary>Changes something, including the workflow operations the API exposes as GET. Never retried.</summary>
    Write,
}

/// <summary>Sends requests to the Management API: auth, User-Agent, retries for reads, and typed errors.</summary>
internal sealed class ManagementConnection
{
    private const int MaxErrorBodyLength = 16 * 1024;

    private static readonly string SdkUserAgent = BuildUserAgent();

    private readonly HttpClient _http;
    private IAccessTokenProvider? _tokens;
    private readonly string _userAgent;

    public ManagementConnection(HttpClient http, AgilityManagementOptions options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        _http = http;
        Options = options;
        Time = timeProvider ?? TimeProvider.System;
        _tokens = options.AccessTokenProvider
            ?? (string.IsNullOrWhiteSpace(options.AccessToken) ? null : new StaticAccessTokenProvider(options.AccessToken));
        _userAgent = string.IsNullOrWhiteSpace(options.ApplicationName)
            ? SdkUserAgent
            : $"{SdkUserAgent} {options.ApplicationName.Trim()}";
    }

    public AgilityManagementOptions Options { get; }

    /// <summary>Set once by the client when the credentials are a refresh token, which needs the client's OAuth endpoints.</summary>
    internal void UseTokenProvider(IAccessTokenProvider provider) => _tokens ??= provider;

    public TimeProvider Time { get; }

    /// <summary><c>{region host}/api/v1/instance/{guid}/{path}{query}</c></summary>
    public Uri InstanceUri(string guid, ApiPath path, Query? query = null)
    {
        var host = Options.BaseUrl ?? AgilityRegions.ResolveBaseUrl(guid);
        return Combine(host, $"api/v1/instance/{Uri.EscapeDataString(guid)}/{path.ToStringAndClear()}", query);
    }

    /// <summary><c>{server host}/api/v1/{path}{query}</c></summary>
    public Uri ServerUri(ApiPath path, Query? query = null) =>
        Combine(Options.BaseUrl ?? AgilityRegions.DefaultServerUrl, $"api/v1/{path.ToStringAndClear()}", query);

    /// <summary><c>{server host}/{path}{query}</c>, for the OAuth endpoints outside <c>/api</c>.</summary>
    public Uri RootUri(ApiPath path, Query? query = null) =>
        Combine(Options.BaseUrl ?? AgilityRegions.DefaultServerUrl, path.ToStringAndClear(), query);

    public async Task<T?> SendAsync<T>(
        HttpMethod method, Uri uri, RequestKind kind, JsonTypeInfo<T> responseType,
        Func<HttpContent>? content = null, bool anonymous = false, CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, uri, kind, content, anonymous, cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync(response, responseType, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Like <see cref="SendAsync{T}"/>, but throws if the API returned an empty body.</summary>
    public async Task<T> SendRequiredAsync<T>(
        HttpMethod method, Uri uri, RequestKind kind, JsonTypeInfo<T> responseType,
        Func<HttpContent>? content = null, bool anonymous = false, CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(method, uri, kind, responseType, content, anonymous, cancellationToken).ConfigureAwait(false);
        return result ?? throw new AgilityManagementException($"{method} {uri.AbsolutePath} returned an empty response.")
        {
            Method = method.Method,
            RequestUri = uri,
        };
    }

    /// <summary>Sends a request and ignores the response body.</summary>
    public async Task SendAsync(
        HttpMethod method, Uri uri, RequestKind kind, Func<HttpContent>? content = null, bool anonymous = false,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, uri, kind, content, anonymous, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Returns the body as text, unwrapping it if the API sent a JSON string.</summary>
    public async Task<string> SendForStringAsync(
        HttpMethod method, Uri uri, RequestKind kind, bool anonymous = false, CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, uri, kind, null, anonymous, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (text.Length > 1 && text[0] == '"')
        {
            try
            {
                return JsonSerializer.Deserialize(text, ManagementJsonContext.Default.String) ?? "";
            }
            catch (JsonException)
            {
                // Not a JSON string after all; return it as sent.
            }
        }
        return text;
    }

    /// <summary>Returns the body as a stream the caller owns.</summary>
    public async Task<Stream> SendForStreamAsync(
        HttpMethod method, Uri uri, RequestKind kind, CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, uri, kind, null, false, cancellationToken).ConfigureAwait(false);
        var buffer = new MemoryStream();
        await response.Content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;
        return buffer;
    }

    public static Func<HttpContent> Json<T>(T value, JsonTypeInfo<T> type) => () => JsonContent.Create(value, type);

    private async Task<HttpResponseMessage> SendCoreAsync(
        HttpMethod method, Uri uri, RequestKind kind, Func<HttpContent>? content, bool anonymous,
        CancellationToken cancellationToken)
    {
        var retries = kind == RequestKind.Read ? Options.Retry.MaxRetries : 0;
        for (var attempt = 0; ; attempt++)
        {
            using var request = new HttpRequestMessage(method, uri);
            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
            if (!anonymous)
            {
                var tokens = _tokens ?? throw new InvalidOperationException(
                    $"{method} {uri.AbsolutePath} needs credentials: set {nameof(AgilityManagementOptions)}.{nameof(AgilityManagementOptions.AccessToken)}, " +
                    $"{nameof(AgilityManagementOptions.RefreshToken)} or {nameof(AgilityManagementOptions.AccessTokenProvider)}.");
                var token = await tokens.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            request.Content = content?.Invoke();

            HttpResponseMessage response;
            try
            {
                // ResponseContentRead buffers the body here, so a body that fails or stalls mid-read is retried
                // (for reads), wrapped, and covered by HttpClient.Timeout like the rest of the request.
                response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                if (attempt < retries)
                {
                    await DelayAsync(attempt, null, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                throw new AgilityManagementException($"{method} {uri.AbsolutePath} failed: {ex.Message}", ex)
                {
                    Method = method.Method,
                    RequestUri = uri,
                    StatusCode = ex.StatusCode,
                };
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // HttpClient.Timeout elapsed.
                if (attempt < retries)
                {
                    await DelayAsync(attempt, null, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                throw new AgilityManagementException($"{method} {uri.AbsolutePath} timed out.", ex)
                {
                    Method = method.Method,
                    RequestUri = uri,
                };
            }

            if (response.IsSuccessStatusCode) return response;

            if (attempt < retries && IsTransient(response.StatusCode))
            {
                var retryAfter = response.Headers.RetryAfter;
                response.Dispose();
                await DelayAsync(attempt, retryAfter, cancellationToken).ConfigureAwait(false);
                continue;
            }

            using (response)
            {
                throw await CreateErrorAsync(method, uri, response, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;

    private Task DelayAsync(int attempt, RetryConditionHeaderValue? retryAfter, CancellationToken cancellationToken)
    {
        var retry = Options.Retry;
        TimeSpan delay;
        if (retryAfter?.Delta is { } delta)
            delay = delta;
        else if (retryAfter?.Date is { } date)
            delay = date - Time.GetUtcNow();
        else
        {
            var backoff = retry.BaseDelay.TotalMilliseconds * Math.Pow(2, attempt);
#pragma warning disable CA5394 // Jitter for retry spacing, not security.
            delay = TimeSpan.FromMilliseconds(backoff + (Random.Shared.NextDouble() * retry.BaseDelay.TotalMilliseconds));
#pragma warning restore CA5394
        }
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
        if (delay > retry.MaxDelay) delay = retry.MaxDelay;
        return delay == TimeSpan.Zero ? Task.CompletedTask : Task.Delay(delay, Time, cancellationToken);
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, JsonTypeInfo<T> type, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(body)) return default;
        try
        {
            return JsonSerializer.Deserialize(body, type);
        }
        catch (JsonException ex)
        {
            var request = response.RequestMessage;
            throw new AgilityManagementException(
                $"{request?.Method} {request?.RequestUri?.AbsolutePath} returned a response that isn't a valid {typeof(T).Name}: {ex.Message}", ex)
            {
                StatusCode = response.StatusCode,
                Method = request?.Method.Method,
                RequestUri = request?.RequestUri,
                ResponseBody = Trim(body),
            };
        }
    }

    private static async Task<AgilityManagementException> CreateErrorAsync(
        HttpMethod method, Uri uri, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string? body = null;
        try
        {
            body = Trim(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }
        catch (HttpRequestException)
        {
            // The status code is still worth reporting without the body.
        }

        var (apiMessage, problem) = ParseErrorBody(body, response.Content.Headers.ContentType?.MediaType);
        var requestId = Header(response, "x-request-id") ?? Header(response, "request-id") ?? Header(response, "x-ms-request-id")
            ?? (problem?.AdditionalProperties?.TryGetValue("traceId", out var trace) == true && trace.ValueKind == JsonValueKind.String
                ? trace.GetString() : null);

        var message = $"{method} {uri.AbsolutePath} returned {(int)response.StatusCode} {response.ReasonPhrase}";
        if (!string.IsNullOrWhiteSpace(apiMessage)) message += $": {apiMessage}";
        if (requestId is not null) message += $" (request ID {requestId})";

        return new AgilityManagementException(message)
        {
            StatusCode = response.StatusCode,
            Method = method.Method,
            RequestUri = uri,
            ResponseBody = body,
            ApiMessage = apiMessage,
            Problem = problem,
            RequestId = requestId,
        };
    }

    internal static (string? Message, ProblemDetails? Problem) ParseErrorBody(string? body, string? mediaType)
    {
        if (string.IsNullOrWhiteSpace(body)) return (null, null);
        var trimmed = body.TrimStart();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('"'))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.String) return (root.GetString(), null);
                if (root.ValueKind == JsonValueKind.Object)
                {
                    ProblemDetails? problem = null;
                    if (mediaType == "application/problem+json" || root.TryGetProperty("title", out _) || root.TryGetProperty("detail", out _))
                        problem = root.Deserialize(ManagementJsonContext.Default.ProblemDetails);
                    var message = Str(root, "detail") ?? Str(root, "error_description") ?? Str(root, "message")
                        ?? Str(root, "title") ?? Str(root, "error");
                    return (message, problem);
                }
            }
            catch (JsonException)
            {
                // Fall through and treat it as text.
            }
        }
        // Plain text: keep it if it's short enough to be a message rather than a page.
        return trimmed.Length <= 1000 && !trimmed.StartsWith('<') ? (trimmed.Trim(), null) : (null, null);
    }

    private static string? Str(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString())
            ? v.GetString() : null;

    private static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;

    private static string Trim(string body) => body.Length <= MaxErrorBodyLength ? body : body[..MaxErrorBodyLength];

    private static Uri Combine(Uri host, string path, Query? query) =>
        new($"{host.AbsoluteUri.TrimEnd('/')}/{path}{query}");

    private static string BuildUserAgent()
    {
        var version = typeof(ManagementConnection).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
        var plus = version.IndexOf('+', StringComparison.Ordinal);
        if (plus >= 0) version = version[..plus];
        return $"agility-management-sdk-dotnet/{version} ({RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription})";
    }
}
