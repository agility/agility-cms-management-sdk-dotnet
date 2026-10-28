using System.Net;
using System.Text.Json.Nodes;
using Agility.Management.Sdk.Clients;
using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk.Tests;

public class SearchTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const string Result = """
        {
          "query": "newsl",
          "relaxed": false,
          "facets": {
            "content": { "count": 6, "capped": false },
            "page": { "count": 120, "capped": true }
          },
          "total": 6,
          "hits": [
            {
              "kind": "content",
              "typeLabel": "List Item",
              "name": "Newsletter sign-up",
              "context": "Posts",
              "snippet": "&lt;b&gt; <mark>Newsl</mark>etter",
              "matchedFields": ["_title", "body"],
              "score": 140,
              "languageCode": "en-us",
              "state": "Published",
              "modifiedDate": "2026-10-01T12:00:00Z",
              "modifiedBy": "Ann",
              "viewLink": "content/list-12/listitem-34",
              "contentID": 34,
              "containerID": 12,
              "containerReferenceName": "posts",
              "modelID": 5,
              "modelReferenceName": "Post"
            },
            {
              "kind": "asset",
              "name": "newsletter.pdf",
              "matchedFields": ["_file"],
              "score": 90,
              "mediaID": 77,
              "url": "https://cdn.example.com/newsletter.pdf",
              "contentType": "application/pdf",
              "size": 5000000000
            }
          ],
          "indexing": { "pending": 3, "catchingUp": true }
        }
        """;

    private static JsonObject SentBody(FakeHandler handler) => JsonNode.Parse(handler.Requests.Single().Body!)!.AsObject();

    [Fact]
    public async Task Search_posts_to_the_instance_search_route_and_reads_the_result()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        var result = await client.Search.SearchAsync(TestClient.InstanceGuid, "newsl", cancellationToken: Ct);

        var request = handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"/api/v1/instance/{TestClient.InstanceGuid}/search", request.Uri.AbsolutePath);
        Assert.Equal("application/json", request.ContentType);

        Assert.Equal("newsl", result.Query);
        Assert.False(result.Relaxed);
        Assert.Equal(6, result.Total);
        Assert.Equal(6, result.Facets![SearchKinds.Content].Count);
        Assert.True(result.Facets[SearchKinds.Page].Capped);
        Assert.Equal(3, result.Indexing!.Pending);
        Assert.True(result.Indexing.CatchingUp);

        var hit = result.Hits![0];
        Assert.Equal(SearchKinds.Content, hit.Kind);
        Assert.Equal("List Item", hit.TypeLabel);
        Assert.Equal(["_title", "body"], hit.MatchedFields);
        Assert.Equal(140, hit.Score);
        Assert.Equal("content/list-12/listitem-34", hit.ViewLink);
        Assert.Equal(34, hit.ContentID);
        Assert.Equal(12, hit.ContainerID);
        Assert.Equal("Post", hit.ModelReferenceName);
        Assert.Equal(new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc), hit.ModifiedDate!.Value.ToUniversalTime());

        var asset = result.Hits[1];
        Assert.Equal(77, asset.MediaID);
        Assert.Equal(5_000_000_000L, asset.Size);
        Assert.Null(asset.LanguageCode);
    }

    [Fact]
    public async Task A_search_with_no_options_sends_the_query_and_the_default_page()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await client.Search.SearchAsync(TestClient.InstanceGuid, "\"exact phrase\"", cancellationToken: Ct);

        // Unset filters are left out, so the API applies its defaults (every kind, every locale).
        var body = SentBody(handler);
        Assert.Equal(["query", "skip", "take"], body.Select(p => p.Key));
        Assert.Equal("\"exact phrase\"", body["query"]!.GetValue<string>());
        Assert.Equal(0, body["skip"]!.GetValue<int>());
        Assert.Equal(25, body["take"]!.GetValue<int>());
    }

    [Fact]
    public async Task Every_option_is_sent_with_the_contracts_camelCase_names()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await client.Search.SearchAsync(TestClient.InstanceGuid, "newsl", new SearchOptions
        {
            Kinds = [SearchKinds.Content, SearchKinds.Page],
            Locale = "en-us",
            Containers = ["posts"],
            Models = ["Post"],
            State = "published",
            ModifiedAfter = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            Skip = 50,
            Take = 10,
        }, Ct);

        var body = SentBody(handler);
        Assert.Equal(
            ["query", "kinds", "locale", "containers", "models", "state", "modifiedAfter", "skip", "take"],
            body.Select(p => p.Key));
        Assert.Equal("newsl", body["query"]!.GetValue<string>());
        Assert.Equal(["content", "page"], body["kinds"]!.AsArray().Select(k => k!.GetValue<string>()));
        Assert.Equal("en-us", body["locale"]!.GetValue<string>());
        Assert.Equal(["posts"], body["containers"]!.AsArray().Select(k => k!.GetValue<string>()));
        Assert.Equal(["Post"], body["models"]!.AsArray().Select(k => k!.GetValue<string>()));
        Assert.Equal("published", body["state"]!.GetValue<string>());
        Assert.Equal("2026-09-01T00:00:00Z", body["modifiedAfter"]!.GetValue<string>());
        Assert.Equal(50, body["skip"]!.GetValue<int>());
        Assert.Equal(10, body["take"]!.GetValue<int>());
    }

    [Fact]
    public async Task An_empty_kinds_list_is_sent_as_an_empty_array()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await client.Search.SearchAsync(TestClient.InstanceGuid, "x", new SearchOptions { Kinds = [] }, Ct);

        Assert.Empty(SentBody(handler)["kinds"]!.AsArray());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task A_blank_query_is_refused_before_any_request(string? query)
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => client.Search.SearchAsync(TestClient.InstanceGuid, query!, cancellationToken: Ct));

        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task A_400_is_an_AgilityManagementException_with_the_APIs_message_and_is_not_retried()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(
            """{"statusCode":400,"exceptionMessage":"Unknown kind 'bogus'. Use content, component, page, asset, model or container.","additionalParameters":null}""",
            HttpStatusCode.BadRequest));
        var client = TestClient.Create(handler, o => o.Retry.MaxRetries = 3);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() =>
            client.Search.SearchAsync(TestClient.InstanceGuid, "x", new SearchOptions { Kinds = ["bogus"] }, Ct));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Unknown kind 'bogus'. Use content, component, page, asset, model or container.", ex.ApiMessage);
        Assert.Contains("Unknown kind 'bogus'", ex.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_501_is_an_AgilitySearchNotAvailableException_and_is_not_retried()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(
            """{"statusCode":501,"exceptionMessage":"Search is not available for this instance yet.","additionalParameters":null}""",
            HttpStatusCode.NotImplemented));
        var client = TestClient.Create(handler, o => o.Retry.MaxRetries = 3);

        var ex = await Assert.ThrowsAsync<AgilitySearchNotAvailableException>(() =>
            client.Search.SearchAsync(TestClient.InstanceGuid, "x", cancellationToken: Ct));

        // Still an AgilityManagementException, with everything the generic one carries.
        Assert.IsAssignableFrom<AgilityManagementException>(ex);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
        Assert.Equal("Search is not available for this instance yet.", ex.ApiMessage);
        Assert.Equal("POST", ex.Method);
        Assert.Equal($"/api/v1/instance/{TestClient.InstanceGuid}/search", ex.RequestUri!.AbsolutePath);
        Assert.Contains("Search is not available", ex.ResponseBody, StringComparison.Ordinal);
        Assert.IsType<AgilityManagementException>(ex.InnerException);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_501_from_any_other_route_stays_a_plain_AgilityManagementException()
    {
        var handler = new FakeHandler(_ => FakeHandler.Text("Not implemented", HttpStatusCode.NotImplemented));
        var client = TestClient.Create(handler);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() => client.Models.GetFieldTypesAsync(TestClient.InstanceGuid, Ct));

        Assert.IsNotType<AgilitySearchNotAvailableException>(ex);
    }

    [Fact]
    public async Task A_transient_failure_is_retried_because_search_only_reads()
    {
        var calls = 0;
        var handler = new FakeHandler(_ => ++calls == 1
            ? FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable)
            : FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        var result = await client.Search.SearchAsync(TestClient.InstanceGuid, "newsl", cancellationToken: Ct);

        Assert.Equal(6, result.Total);
        Assert.Equal(2, handler.Requests.Count);
    }
}
