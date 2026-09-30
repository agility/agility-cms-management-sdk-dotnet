using System.Net;
using System.Text;

namespace Agility.Management.Sdk.Tests;

/// <summary>A request the SDK sent, captured with its body.</summary>
public sealed record RecordedRequest(HttpMethod Method, Uri Uri, HttpRequestHeadersSnapshot Headers, string? Body, string? ContentType);

/// <summary>The request headers the tests look at.</summary>
public sealed record HttpRequestHeadersSnapshot(string? Authorization, string? UserAgent, string? Accept);

/// <summary>
/// Answers every request from a function and records it. No request leaves the process.
/// </summary>
public sealed class FakeHandler : HttpMessageHandler
{
    private readonly Func<RecordedRequest, HttpResponseMessage> _respond;

    public FakeHandler(Func<RecordedRequest, HttpResponseMessage> respond) => _respond = respond;

    public List<RecordedRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var recorded = new RecordedRequest(
            request.Method,
            request.RequestUri!,
            new HttpRequestHeadersSnapshot(
                request.Headers.Authorization?.ToString(),
                request.Headers.TryGetValues("User-Agent", out var ua) ? string.Join(" ", ua) : null,
                request.Headers.Accept.ToString()),
            body,
            request.Content?.Headers.ContentType?.MediaType);
        lock (Requests) Requests.Add(recorded);
        var response = _respond(recorded);
        response.RequestMessage = request;
        return response;
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public static HttpResponseMessage Text(string text, HttpStatusCode status) =>
        new(status) { Content = new StringContent(text, Encoding.UTF8, "text/plain") };
}

public static class TestClient
{
    public const string InstanceGuid = "1234abcd-u";

    /// <summary>Builds a client over <paramref name="handler"/> with fast, test-sized timings.</summary>
    public static AgilityManagementClient Create(FakeHandler handler, Action<AgilityManagementOptions>? configure = null)
    {
        var options = new AgilityManagementOptions
        {
            AccessToken = "test-token",
            Retry = { BaseDelay = TimeSpan.Zero, MaxDelay = TimeSpan.FromMilliseconds(10) },
            BatchPolling = { Interval = TimeSpan.FromMilliseconds(1), Timeout = TimeSpan.FromSeconds(5), NotFoundGracePeriod = TimeSpan.FromSeconds(5) },
        };
        configure?.Invoke(options);
        return new AgilityManagementClient(options, new HttpClient(handler));
    }

    public static AgilityInstanceClient Instance(FakeHandler handler, Action<AgilityManagementOptions>? configure = null) =>
        Create(handler, configure).ForInstance(InstanceGuid);

    /// <summary>A processed batch with one successful item.</summary>
    public static string ProcessedBatch(int batchId, int itemId = 555) =>
        $$"""{"batchID":{{batchId}},"batchState":3,"items":[{"batchItemID":1,"batchID":{{batchId}},"itemType":2,"itemID":{{itemId}},"errorMessage":null}]}""";
}
