using System.Net;

namespace Agility.Management.Sdk.Tests;

public class MovePageTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private const string Result = """{"pageID":8,"parentPageID":7,"versionID":580,"moved":true,"siblingPageIDs":[15,13,11,8,10,9]}""";

    [Fact]
    public async Task Posts_to_the_pages_move_route_with_the_parent_and_sibling_and_reads_the_result()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        var result = await client.Pages.MovePageAsync(TestClient.InstanceGuid, "en-us", 8, parentPageId: 7, placeBeforePageId: 10, cancellationToken: Ct);

        var request = handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"/api/v1/instance/{TestClient.InstanceGuid}/en-us/page/8/move", request.Uri.AbsolutePath);
        Assert.Equal("?parentPageID=7&placeBeforePageID=10", request.Uri.Query);

        Assert.Equal(8, result.PageID);
        Assert.Equal(7, result.ParentPageID);
        Assert.Equal(580, result.VersionID);
        Assert.True(result.Moved);
        Assert.Equal([15, 13, 11, 8, 10, 9], result.SiblingPageIDs);
    }

    [Fact]
    public async Task Without_a_parent_or_sibling_moves_to_the_root_last()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await client.Pages.MovePageAsync(TestClient.InstanceGuid, "en-us", 8, cancellationToken: Ct);

        Assert.Equal("?parentPageID=-1&placeBeforePageID=-1", handler.Requests.Single().Uri.Query);
    }

    [Fact]
    public async Task A_page_already_in_place_reads_as_not_moved()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(
            """{"pageID":8,"parentPageID":7,"versionID":579,"moved":false,"siblingPageIDs":[15,13,11,8,10,9]}"""));
        var client = TestClient.Create(handler);

        var result = await client.Pages.MovePageAsync(TestClient.InstanceGuid, "en-us", 8, 7, 10, Ct);

        Assert.False(result.Moved);
    }

    [Fact]
    public async Task A_400_carries_the_APIs_message_and_is_not_retried()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(
            "\"Parent page 80 is under page 7. A page can't be moved under one of its own descendants.\"", HttpStatusCode.BadRequest));
        var client = TestClient.Create(handler, o => o.Retry.MaxRetries = 3);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() =>
            client.Pages.MovePageAsync(TestClient.InstanceGuid, "en-us", 7, 80, cancellationToken: Ct));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Contains("own descendants", ex.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_transient_failure_is_not_retried_because_it_writes()
    {
        var handler = new FakeHandler(_ => FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable));
        var client = TestClient.Create(handler, o => o.Retry.MaxRetries = 3);

        await Assert.ThrowsAsync<AgilityManagementException>(() =>
            client.Pages.MovePageAsync(TestClient.InstanceGuid, "en-us", 8, 7, 10, Ct));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Rejects_a_missing_locale_without_calling_the_API()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(Result));
        var client = TestClient.Create(handler);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.Pages.MovePageAsync(TestClient.InstanceGuid, " ", 8, cancellationToken: Ct));
        Assert.Empty(handler.Requests);
    }
}
