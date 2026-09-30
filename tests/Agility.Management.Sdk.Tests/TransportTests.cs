using System.Net;
using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk.Tests;

public class TransportTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Requests_carry_the_bearer_token_an_identifying_user_agent_and_accept_json()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        var instance = TestClient.Instance(handler, o => o.ApplicationName = "my-job/1.0");

        await instance.Models.GetFieldTypesAsync(Ct);

        var headers = handler.Requests.Single().Headers;
        Assert.Equal("Bearer test-token", headers.Authorization);
        Assert.Matches(@"^agility-management-sdk-dotnet/\d+\.\d+\.\d+\S* \(\.NET [^;]+; [^)]+\) my-job/1\.0$", headers.UserAgent);
        Assert.Contains("application/json", headers.Accept, StringComparison.Ordinal);
        Assert.Matches(@"^agility-management-sdk-dotnet/\d+\.\d+\.\d+\S*$", headers.Sdk);
    }

    [Fact]
    public async Task A_token_provider_is_asked_for_a_token_on_each_request()
    {
        var provider = new CountingTokenProvider();
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        var instance = TestClient.Instance(handler, o => o.AccessTokenProvider = provider);

        await instance.Models.GetFieldTypesAsync(Ct);
        await instance.Models.GetFieldTypesAsync(Ct);

        Assert.Equal(["Bearer token-1", "Bearer token-2"], handler.Requests.Select(r => r.Headers.Authorization));
    }

    [Fact]
    public async Task Path_values_are_escaped_as_single_segments()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        var instance = TestClient.Instance(handler);

        await instance.Containers.GetContainerByReferenceNameAsync("a b/../c?d", Ct);

        Assert.Equal($"/api/v1/instance/{TestClient.InstanceGuid}/container/a%20b%2F..%2Fc%3Fd", handler.Requests.Single().Uri.AbsolutePath);
    }

    [Fact]
    public async Task Query_values_are_escaped_and_null_parameters_are_left_out()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        var instance = TestClient.Instance(handler);

        await instance.Users.SaveUserAsync("a+b@example.com", [], firstName: "Ann Marie", cancellationToken: Ct);

        Assert.Equal("?emailAddress=a%2Bb%40example.com&firstName=Ann%20Marie", handler.Requests.Single().Uri.Query);
    }

    [Fact]
    public async Task Query_booleans_are_lowercase_and_dates_are_ISO_8601()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        var instance = TestClient.Instance(handler);

        await instance.Models.GetContentModelsAsync(includeDefaults: true, includeModules: false,
            updatedSince: new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), cancellationToken: Ct);

        var uri = handler.Requests.Single().Uri;
        Assert.EndsWith("/model/list/true", uri.AbsolutePath, StringComparison.Ordinal);
        Assert.Equal("?includeModules=false&updatedSince=2026-01-02T03%3A04%3A05.0000000Z", uri.Query);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task Reads_are_retried_after_a_transient_failure(HttpStatusCode status)
    {
        var calls = 0;
        var handler = new FakeHandler(_ => ++calls < 3 ? FakeHandler.Text("busy", status) : FakeHandler.Json("""["Text"]"""));
        var instance = TestClient.Instance(handler);

        var types = await instance.Models.GetFieldTypesAsync(Ct);

        Assert.Equal(["Text"], types);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task Reads_stop_retrying_after_MaxRetries_and_report_the_last_error()
    {
        var handler = new FakeHandler(_ => FakeHandler.Text("down for maintenance", HttpStatusCode.ServiceUnavailable));
        var instance = TestClient.Instance(handler, o => o.Retry.MaxRetries = 2);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() => instance.Models.GetFieldTypesAsync(Ct));

        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.Equal("down for maintenance", ex.ApiMessage);
    }

    [Fact]
    public async Task A_workflow_GET_is_never_retried_because_repeating_it_repeats_the_change()
    {
        var handler = new FakeHandler(_ => FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable));
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAsync<AgilityManagementException>(() =>
            instance.Content.PublishContentItemAsync("en-us", 7, cancellationToken: Ct));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_save_is_never_retried()
    {
        var handler = new FakeHandler(_ => FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable));
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAsync<AgilityManagementException>(() =>
            instance.Content.SaveContentItemAsync("en-us", new ContentItem(), cancellationToken: Ct));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_content_list_POST_is_a_read_and_is_retried()
    {
        var calls = 0;
        var handler = new FakeHandler(_ => ++calls == 1 ? FakeHandler.Text("busy", HttpStatusCode.ServiceUnavailable) : FakeHandler.Json("""{"totalCount":0,"items":[]}"""));
        var instance = TestClient.Instance(handler);

        await instance.Content.GetContentListAsync("en-us", "posts", cancellationToken: Ct);

        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, r => Assert.Equal(HttpMethod.Post, r.Method));
    }

    [Fact]
    public async Task Plain_text_errors_become_the_exception_message()
    {
        var handler = new FakeHandler(_ => FakeHandler.Text("Locale with ID 9 was not found.", HttpStatusCode.NotFound));
        var instance = TestClient.Instance(handler);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() => instance.Locales.GetLocaleAsync(9, Ct));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Equal("Locale with ID 9 was not found.", ex.ApiMessage);
        Assert.Contains("GET /api/v1/instance/1234abcd-u/locales/9 returned 404", ex.Message, StringComparison.Ordinal);
        Assert.Equal("GET", ex.Method);
    }

    [Fact]
    public async Task OAuth_style_error_bodies_use_their_description()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("""{"error":"rate_limit_exceeded","error_description":"5 tokens per hour"}""", HttpStatusCode.TooManyRequests));
        var client = TestClient.Create(handler, o => o.Retry.MaxRetries = 0);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() =>
            client.PersonalAccessTokens.CreateTokenAsync(new PersonalAccessTokenRequest { Name = "ci" }, Ct));

        Assert.Equal("5 tokens per hour", ex.ApiMessage);
    }

    [Fact]
    public async Task Problem_details_are_parsed_and_the_request_id_is_kept()
    {
        var handler = new FakeHandler(_ =>
        {
            var r = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"title":"One or more validation errors occurred.","status":400,"traceId":"00-abc-01"}""",
                    System.Text.Encoding.UTF8, "application/problem+json"),
            };
            return r;
        });
        var instance = TestClient.Instance(handler);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() =>
            instance.Models.SaveModelAsync(new ContentModel(), Ct));

        Assert.Equal("One or more validation errors occurred.", ex.Problem?.Title);
        Assert.Equal("00-abc-01", ex.RequestId);
    }

    [Fact]
    public async Task Validation_errors_are_listed_in_the_message()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(
            """{"title":"One or more validation errors occurred.","status":400,"errors":{"GenericSearch":["The GenericSearch field is required."]}}""",
            HttpStatusCode.BadRequest));
        var instance = TestClient.Instance(handler);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() => instance.Models.SaveModelAsync(new ContentModel(), Ct));

        Assert.Equal("One or more validation errors occurred. The GenericSearch field is required.", ex.ApiMessage);
    }

    [Fact]
    public async Task Network_failures_are_wrapped_with_the_original_exception_kept()
    {
        var inner = new HttpRequestException("connection refused");
        var handler = new FakeHandler(_ => throw inner);
        var instance = TestClient.Instance(handler, o => o.Retry.MaxRetries = 1);

        var ex = await Assert.ThrowsAsync<AgilityManagementException>(() => instance.Models.GetFieldTypesAsync(Ct));

        Assert.Same(inner, ex.InnerException);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Cancellation_is_not_wrapped_or_retried()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => instance.Models.GetFieldTypesAsync(cts.Token));
    }

    [Fact]
    public async Task An_empty_body_where_a_value_is_required_is_an_error_not_a_null()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var instance = TestClient.Instance(handler);

        await Assert.ThrowsAsync<AgilityManagementException>(() => instance.Pages.GetPageAsync("en-us", 1, Ct));
    }

    [Fact]
    public async Task A_BaseUrl_override_is_used_for_instance_and_server_calls()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        var client = TestClient.Create(handler, o => o.BaseUrl = new Uri("http://localhost:5050/"));

        await client.ForInstance("1234abcd-zz").Assets.GetDefaultContainerAsync(Ct);
        await client.Users.GetCurrentUserAsync(Ct);

        Assert.Equal("http://localhost:5050/api/v1/instance/1234abcd-zz/asset/container", handler.Requests[0].Uri.ToString());
        Assert.Equal("http://localhost:5050/api/v1/users/me", handler.Requests[1].Uri.ToString());
    }

    [Fact]
    public async Task Anonymous_endpoints_send_no_token()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        var client = TestClient.Create(handler);

        await client.Types.GetAllTypesAsync(Ct);

        Assert.Null(handler.Requests.Single().Headers.Authorization);
    }

    [Fact]
    public async Task The_refresh_token_is_sent_in_the_body_not_the_URL()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("""{"access_token":"a","expires_in":3600}"""));
        var client = TestClient.Create(handler);

        await client.OAuth.RefreshAsync("secret-refresh", Ct);

        var request = handler.Requests.Single();
        Assert.DoesNotContain("secret-refresh", request.Uri.ToString(), StringComparison.Ordinal);
        Assert.Equal("refresh_token=secret-refresh", request.Body);
        Assert.Null(request.Headers.Authorization);
    }

    private sealed class CountingTokenProvider : IAccessTokenProvider
    {
        private int _count;

        public ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken) =>
            ValueTask.FromResult($"token-{Interlocked.Increment(ref _count)}");
    }
}
