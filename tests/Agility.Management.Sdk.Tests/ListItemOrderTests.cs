using System.Net;
using System.Text.Json.Nodes;

namespace Agility.Management.Sdk.Tests;

public class ListItemOrderTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const string Result = """{"contentIDs":[12,9,15],"itemsUpdated":2,"updatedContentIDs":[12,9],"skippedContentIDs":[]}""";

    [Fact]
    public async Task Posts_the_ids_in_order_to_the_lists_item_order_route_and_reads_the_result()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        var result = await client.Content.SetContentListItemOrderAsync(TestClient.InstanceGuid, "en-us", "footer_links", [12, 9], cancellationToken: Ct);

        var request = handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"/api/v1/instance/{TestClient.InstanceGuid}/en-us/list/footer_links/item-order", request.Uri.AbsolutePath);
        Assert.Equal("application/json", request.ContentType);
        Assert.Equal([12, 9], JsonNode.Parse(request.Body!)!.AsArray().Select(n => n!.GetValue<int>()));

        Assert.Equal("", request.Uri.Query);

        Assert.Equal([12, 9, 15], result.ContentIDs);
        Assert.Equal(2, result.ItemsUpdated);
        Assert.Equal([12, 9], result.UpdatedContentIDs);
        Assert.Empty(result.SkippedContentIDs!);
    }

    [Fact]
    public async Task SkipUnknownIds_sends_the_flag_and_reads_the_skipped_ids()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(
            """{"contentIDs":[12,9],"itemsUpdated":1,"updatedContentIDs":[12],"skippedContentIDs":[99]}"""));
        var client = TestClient.Create(handler);

        var result = await client.Content.SetContentListItemOrderAsync(
            TestClient.InstanceGuid, "en-us", "footer_links", [12, 99], skipUnknownIds: true, cancellationToken: Ct);

        Assert.Equal("?skipUnknownIds=true", handler.Requests.Single().Uri.Query);
        Assert.Equal([99], result.SkippedContentIDs);
    }

    [Fact]
    public async Task Escapes_the_reference_name()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await client.Content.SetContentListItemOrderAsync(TestClient.InstanceGuid, "en-us", "a b/c", [1], cancellationToken: Ct);

        Assert.EndsWith("/en-us/list/a%20b%2Fc/item-order", handler.Requests.Single().Uri.AbsoluteUri, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_400_carries_the_APIs_message_and_is_not_retried()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(
            "\"These content IDs aren't items in footer_links for locale en-us: 99.\"", HttpStatusCode.BadRequest));
        var client = TestClient.Create(handler, o => o.Retry.MaxRetries = 3);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() =>
            client.Content.SetContentListItemOrderAsync(TestClient.InstanceGuid, "en-us", "footer_links", [1, 99], cancellationToken: Ct));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Contains("footer_links for locale en-us: 99", ex.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_transient_failure_is_not_retried_because_it_writes()
    {
        var handler = new FakeHandler(_ => FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable));
        var client = TestClient.Create(handler, o => o.Retry.MaxRetries = 3);

        await Assert.ThrowsAsync<AgilityManagementException>(() =>
            client.Content.SetContentListItemOrderAsync(TestClient.InstanceGuid, "en-us", "footer_links", [1], cancellationToken: Ct));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Rejects_a_missing_locale_or_reference_name_without_calling_the_API()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Content.SetContentListItemOrderAsync(TestClient.InstanceGuid, " ", "footer_links", [1], cancellationToken: Ct));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Content.SetContentListItemOrderAsync(TestClient.InstanceGuid, "en-us", "", [1], cancellationToken: Ct));
        Assert.Empty(handler.Requests);
    }
}
