using System.Net;

namespace Agility.Management.Sdk.Tests;

public class BatchTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static bool IsBatchGet(RecordedRequest r) =>
        r.Method == HttpMethod.Get && r.Uri.AbsolutePath.Contains("/batch/", StringComparison.Ordinal);

    [Fact]
    public async Task A_save_waits_for_its_batch_and_returns_the_new_item_ID()
    {
        var polls = 0;
        var handler = new FakeHandler(r =>
        {
            if (!IsBatchGet(r)) return FakeHandler.Json("88");
            return ++polls < 3
                ? FakeHandler.Json("""{"batchID":88,"batchState":2}""")
                : FakeHandler.Json(TestClient.ProcessedBatch(88, itemId: 1001));
        });
        var client = TestClient.Create(handler);

        var result = await client.Content.SaveContentItemAsync(TestClient.InstanceGuid, "en-us", new Models.ContentItem(), cancellationToken: Ct);

        Assert.Equal(88, result.BatchId);
        Assert.True(result.IsProcessed);
        Assert.Equal(1001, result.ItemId);
        Assert.Equal(3, polls);
        Assert.Contains("expandItems=true", handler.Requests.Last().Uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task waitForBatch_false_returns_the_batch_ID_without_polling()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("88"));
        var client = TestClient.Create(handler);

        var result = await client.Pages.PublishPageAsync(TestClient.InstanceGuid, "en-us", 5, waitForBatch: false, cancellationToken: Ct);

        Assert.Equal(88, result.BatchId);
        Assert.Null(result.Batch);
        Assert.False(result.IsProcessed);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_new_batch_that_404s_briefly_is_treated_as_not_started()
    {
        var polls = 0;
        var handler = new FakeHandler(r =>
        {
            if (!IsBatchGet(r)) return FakeHandler.Json("88");
            return ++polls <= 2 ? FakeHandler.Text("not found", HttpStatusCode.NotFound) : FakeHandler.Json(TestClient.ProcessedBatch(88));
        });
        var client = TestClient.Create(handler);

        var result = await client.Content.PublishContentItemAsync(TestClient.InstanceGuid, "en-us", 7, cancellationToken: Ct);

        Assert.True(result.IsProcessed);
    }

    [Fact]
    public async Task A_batch_that_stays_404_past_the_grace_period_reports_the_404()
    {
        var handler = new FakeHandler(r => IsBatchGet(r) ? FakeHandler.Text("nope", HttpStatusCode.NotFound) : FakeHandler.Json("88"));
        var client = TestClient.Create(handler, o => o.BatchPolling.NotFoundGracePeriod = TimeSpan.FromMilliseconds(20));

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() => client.Content.PublishContentItemAsync(TestClient.InstanceGuid, "en-us", 7, cancellationToken: Ct));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task A_batch_that_never_finishes_times_out_with_its_ID_and_last_state()
    {
        var handler = new FakeHandler(r => IsBatchGet(r) ? FakeHandler.Json("""{"batchID":88,"batchState":2}""") : FakeHandler.Json("88"));
        var client = TestClient.Create(handler, o => o.BatchPolling.Timeout = TimeSpan.FromMilliseconds(50));

        var ex = await Assert.ThrowsAsync<AgilityBatchTimeoutException>(() => client.Content.PublishContentItemAsync(TestClient.InstanceGuid, "en-us", 7, cancellationToken: Ct));

        Assert.Equal(88, ex.BatchId);
        Assert.Equal(Models.BatchState.InProcess, ex.Batch?.BatchState);
        Assert.True(ex.Waited >= TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task The_polling_budget_is_the_configured_timeout_not_half_of_it()
    {
        // 1.x decremented its retry counter twice per poll, so it gave up at half the configured budget.
        var handler = new FakeHandler(r => IsBatchGet(r) ? FakeHandler.Json("""{"batchID":88,"batchState":1}""") : FakeHandler.Json("88"));
        var client = TestClient.Create(handler, o =>
        {
            o.BatchPolling.Interval = TimeSpan.FromMilliseconds(20);
            o.BatchPolling.Timeout = TimeSpan.FromMilliseconds(400);
        });

        var ex = await Assert.ThrowsAsync<AgilityBatchTimeoutException>(() => client.Content.PublishContentItemAsync(TestClient.InstanceGuid, "en-us", 7, cancellationToken: Ct));

        Assert.True(ex.Waited >= TimeSpan.FromMilliseconds(400), $"waited {ex.Waited}");
    }

    [Fact]
    public async Task Failed_items_raise_a_batch_exception_that_keeps_the_batch()
    {
        const string failed = """
            {"batchID":88,"batchState":3,"errorData":"{\"TotalItems\":2,\"SuccessCount\":1,\"FailureCount\":1}",
             "items":[{"itemID":1,"errorMessage":null},{"itemID":2,"errorMessage":"{\"Message\":\"Field 'title' is required\"}"}]}
            """;
        var handler = new FakeHandler(r => IsBatchGet(r) ? FakeHandler.Json(failed) : FakeHandler.Json("88"));
        var client = TestClient.Create(handler);

        var ex = await Assert.ThrowsAsync<AgilityBatchException>(() => client.Content.SaveContentItemsAsync(TestClient.InstanceGuid, "en-us", [new(), new()], cancellationToken: Ct));

        Assert.Equal(88, ex.BatchId);
        Assert.Contains("1 failed item", ex.Message, StringComparison.Ordinal);
        Assert.Contains("item 2: Field 'title' is required", ex.Message, StringComparison.Ordinal);
        Assert.Equal(2, ex.Batch?.Items?.Count);
    }

    [Fact]
    public async Task An_aborted_batch_is_a_failure()
    {
        var handler = new FakeHandler(r => IsBatchGet(r)
            ? FakeHandler.Json("""{"batchID":88,"batchState":3,"abortYN":true,"errorData":"Batch aborted.","items":[]}""")
            : FakeHandler.Json("88"));
        var client = TestClient.Create(handler);

        var ex = await Assert.ThrowsAsync<AgilityBatchException>(() => client.Content.PublishContentItemAsync(TestClient.InstanceGuid, "en-us", 7, cancellationToken: Ct));

        Assert.Contains("aborted", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_missing_batch_ID_is_reported_rather_than_polled()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("null"));
        var client = TestClient.Create(handler);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() => client.Content.PublishContentItemAsync(TestClient.InstanceGuid, "en-us", 7, cancellationToken: Ct));

        Assert.Contains("didn't return a batch ID", ex.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Waiting_can_be_cancelled()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var handler = new FakeHandler(r => IsBatchGet(r) ? FakeHandler.Json("""{"batchID":88,"batchState":1}""") : FakeHandler.Json("88"));
        var client = TestClient.Create(handler, o => o.BatchPolling.Timeout = TimeSpan.FromMinutes(5));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.Batches.WaitForBatchAsync(TestClient.InstanceGuid, 88, cts.Token));
    }
}
