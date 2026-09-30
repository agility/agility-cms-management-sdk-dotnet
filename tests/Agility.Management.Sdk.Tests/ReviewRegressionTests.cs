using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Agility.Management.Sdk.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Agility.Management.Sdk.Tests;

/// <summary>Retry policy for every method, and cases found in review.</summary>
public partial class ReviewRegressionTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static bool IsBatchGet(RecordedRequest r) =>
        r.Method == HttpMethod.Get && r.Uri.AbsolutePath.Contains("/batch/", StringComparison.Ordinal);

    [Fact]
    public async Task Every_method_retries_only_if_it_only_reads()
    {
        const int maxRetries = 2;
        var handler = new FakeHandler(_ => FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable));
        using var client = TestClient.Create(handler, o => o.Retry.MaxRetries = maxRetries);
        var targets = SpecCoverageTests.Targets(client, client.ForInstance(TestClient.InstanceGuid));
        var wrong = new List<string>();

        foreach (var type in SpecCoverageTests.ClientTypes)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .Where(m => typeof(Task).IsAssignableFrom(m.ReturnType)))
            {
                var before = handler.Requests.Count;
                try
                {
                    await (Task)method.Invoke(targets[type], [.. method.GetParameters().Select(SpecCoverageTests.DummyValue)])!;
                }
                catch (AgilityManagementException)
                {
                    // Expected: every attempt fails.
                }
                var sent = handler.Requests.Skip(before).ToList();
                var first = sent[0];
                // The API's reads: GETs that aren't workflow verbs, and the content list query.
                var isRead = (first.Method == HttpMethod.Get && !WorkflowVerb().IsMatch(first.Uri.AbsolutePath))
                    || first.Uri.AbsolutePath.Contains("/list/", StringComparison.Ordinal) && first.Method == HttpMethod.Post;
                var expected = isRead ? maxRetries + 1 : 1;
                if (sent.Count != expected)
                    wrong.Add($"{type.Name}.{method.Name}: {first.Method} {first.Uri.AbsolutePath} sent {sent.Count}, expected {expected}");
            }
        }

        Assert.True(wrong.Count == 0, string.Join("\n", wrong));
    }

    [GeneratedRegex("/(publish|unpublish|approve|decline|request-approval)$")]
    private static partial Regex WorkflowVerb();

    [Fact]
    public async Task A_batch_aborted_through_ErrorData_alone_is_a_failure()
    {
        var handler = new FakeHandler(r => IsBatchGet(r)
            ? FakeHandler.Json("""{"batchID":88,"batchState":3,"errorData":"Batch aborted.","items":[]}""")
            : FakeHandler.Json("88"));
        var instance = TestClient.Instance(handler);

        var ex = await Assert.ThrowsAsync<AgilityBatchException>(() => instance.Content.PublishContentItemAsync("en-us", 7, cancellationToken: Ct));

        Assert.Contains("aborted", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_batch_aborted_before_it_finishes_fails_at_once_instead_of_timing_out()
    {
        var handler = new FakeHandler(r => IsBatchGet(r)
            ? FakeHandler.Json("""{"batchID":88,"batchState":2,"abortYN":true}""")
            : FakeHandler.Json("88"));
        var instance = TestClient.Instance(handler, o => o.BatchPolling.Timeout = TimeSpan.FromMinutes(5));

        var ex = await Assert.ThrowsAsync<AgilityBatchException>(() => instance.Content.PublishContentItemAsync("en-us", 7, cancellationToken: Ct));

        Assert.IsNotType<AgilityBatchTimeoutException>(ex);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Theory]
    [InlineData("""{"FailureCount":null}""")]
    [InlineData("""{"FailureCount":"1"}""")]
    [InlineData("""[1,2]""")]
    public async Task An_unreadable_error_summary_is_ignored_rather_than_thrown(string errorData)
    {
        var batch = $$"""{"batchID":88,"batchState":3,"errorData":{{System.Text.Json.JsonSerializer.Serialize(errorData)}},"items":[{"itemID":5}]}""";
        var handler = new FakeHandler(r => IsBatchGet(r) ? FakeHandler.Json(batch) : FakeHandler.Json("88"));
        var instance = TestClient.Instance(handler);

        var result = await instance.Content.PublishContentItemAsync("en-us", 5, cancellationToken: Ct);

        Assert.True(result.IsProcessed);
    }

    [Fact]
    public async Task Creating_a_batch_without_an_operation_is_refused_instead_of_publishing()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => instance.Batches.CreateBatchAsync(new CreateBatchWithItemsRequest
        {
            Items = [new AddBatchItemRequest { ItemType = BatchItemType.Page, ItemID = 1 }],
        }, processNow: true, Ct));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task A_read_whose_body_fails_midway_is_retried()
    {
        var calls = 0;
        var handler = new FakeHandler(_ => ++calls == 1
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new FailingContent() }
            : FakeHandler.Json("""["Text"]"""));
        var instance = TestClient.Instance(handler);

        var types = await instance.Models.GetFieldTypesAsync(Ct);

        Assert.Equal(["Text"], types);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task A_write_whose_body_fails_is_wrapped_not_retried()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new FailingContent() });
        var instance = TestClient.Instance(handler);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() =>
            instance.Content.PublishContentItemAsync("en-us", 7, waitForBatch: false, cancellationToken: Ct));

        Assert.IsType<HttpRequestException>(ex.InnerException);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Cancelling_during_a_request_stops_it_without_retrying()
    {
        using var cts = new CancellationTokenSource();
        var handler = new FakeHandler(_ =>
        {
            cts.Cancel();
            return FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable);
        });
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => instance.Models.GetFieldTypesAsync(cts.Token));
        Assert.Single(handler.Requests);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("..")]
    public async Task Dot_segments_are_rejected_so_they_cant_change_the_route(string id)
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => instance.Webhooks.DeleteWebhookAsync(id, Ct));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Empty_ID_lists_are_rejected()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => instance.Content.GetContentItemsByIdAsync("en-us", [], Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => instance.Content.BatchWorkflowContentItemsAsync("en-us", [], WorkflowOperationType.Publish, cancellationToken: Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => instance.Pages.BatchWorkflowPagesAsync("en-us", [], WorkflowOperationType.Publish, cancellationToken: Ct));
        await Assert.ThrowsAsync<ArgumentException>(() => instance.UrlRedirections.DeleteUrlRedirectionsAsync([], Ct));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task A_locale_write_with_an_empty_success_body_doesnt_throw()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var instance = TestClient.Instance(handler);

        Assert.Null(await instance.Locales.EnableLocaleAsync(3, Ct));
    }

    [Fact]
    public async Task Short_lived_tokens_are_not_refreshed_on_every_request()
    {
        var refreshes = 0;
        using var provider = new RefreshTokenAccessTokenProvider((_, _) =>
        {
            refreshes++;
            return Task.FromResult(new TokenResponseData { AccessToken = "a", ExpiresIn = 100 });
        }, "r", null);

        for (var i = 0; i < 3; i++) await provider.GetAccessTokenAsync(Ct);

        Assert.Equal(1, refreshes);
    }

    [Fact]
    public async Task A_rotation_handler_can_call_back_into_the_provider()
    {
        RefreshTokenAccessTokenProvider? provider = null;
        var reentered = false;
        provider = new RefreshTokenAccessTokenProvider((r, _) =>
            Task.FromResult(new TokenResponseData { AccessToken = "a", ExpiresIn = 3600, RefreshToken = r + "+" }), "r", null);
        provider.RefreshTokenChanged += (_, _) => reentered = provider.GetAccessTokenAsync(CancellationToken.None).AsTask().Wait(TimeSpan.FromSeconds(5));
        using (provider)
        {
            await provider.GetAccessTokenAsync(Ct);
        }

        Assert.True(reentered);
    }

    [Fact]
    public async Task With_DI_every_resolved_client_shares_one_refresh_token_provider()
    {
        var refreshes = 0;
        string? stored = null;
        var handler = new FakeHandler(r =>
        {
            if (r.Uri.AbsolutePath != "/oauth/refresh") return FakeHandler.Json("{}");
            refreshes++;
            return FakeHandler.Json($$"""{"access_token":"a{{refreshes}}","expires_in":3600,"refresh_token":"r{{refreshes}}"}""");
        });
        var services = new ServiceCollection();
        services.AddAgilityManagement(o =>
        {
            o.RefreshToken = "r0";
            o.RefreshTokenChanged = t => stored = t;
        }).ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var sp = services.BuildServiceProvider();

        await sp.GetRequiredService<AgilityManagementClient>().Users.GetCurrentUserAsync(Ct);
        await sp.GetRequiredService<AgilityManagementClient>().Users.GetCurrentUserAsync(Ct);

        Assert.Equal(1, refreshes);
        Assert.Equal("r1", stored);
        Assert.All(handler.Requests.Where(r => r.Uri.AbsolutePath.EndsWith("/me", StringComparison.Ordinal)),
            r => Assert.Equal("Bearer a1", r.Headers.Authorization));
    }

    private sealed class FailingContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context) =>
            throw new IOException("connection reset");

        protected override bool TryComputeLength(out long length)
        {
            length = 100;
            return true;
        }
    }
}
