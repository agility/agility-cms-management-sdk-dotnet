using Agility.Management.Sdk.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Agility.Management.Sdk.Tests;

public class RegionTests
{
    [Theory]
    [InlineData("1234abcd-u", "https://mgmt.aglty.io/")]
    [InlineData("1234abcd-us2", "https://mgmt-usa2.aglty.io/")]
    [InlineData("1234abcd-US2", "https://mgmt-usa2.aglty.io/")]
    [InlineData("1234abcd-c", "https://mgmt-ca.aglty.io/")]
    [InlineData("1234abcd-e", "https://mgmt-eu.aglty.io/")]
    [InlineData("1234abcd-a", "https://mgmt-aus.aglty.io/")]
    [InlineData("1234abcd-d", "https://mgmt-dev.aglty.io/")]
    [InlineData("1234abcd", "https://mgmt.aglty.io/")]
    public void The_GUID_suffix_selects_the_region(string instanceGuid, string expected) =>
        Assert.Equal(expected, AgilityRegions.ResolveBaseUrl(instanceGuid).ToString());

    [Theory]
    [InlineData("1234abcd-x")]
    [InlineData("1234abcd-usa")]
    [InlineData("1234abcd-u-2")]
    public void An_unknown_suffix_is_an_error_not_a_silent_USA_default(string instanceGuid)
    {
        var ex = Assert.Throws<ArgumentException>(() => AgilityRegions.ResolveBaseUrl(instanceGuid));
        Assert.Contains(instanceGuid, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_bad_GUID_fails_before_any_request_is_sent()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        using var client = TestClient.Create(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => client.Models.GetFieldTypesAsync("1234abcd-x", TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.Models.GetFieldTypesAsync(" ", TestContext.Current.CancellationToken));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task Any_suffix_is_accepted_when_a_BaseUrl_is_set()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        using var client = TestClient.Create(handler, o => o.BaseUrl = new Uri("http://localhost:1"));

        await client.Models.GetFieldTypesAsync("1234abcd-x", TestContext.Current.CancellationToken);

        Assert.Equal("http://localhost:1/api/v1/instance/1234abcd-x/model/field-types", handler.Requests.Single().Uri.ToString());
    }
}

public class OptionsTests
{
    [Fact]
    public void A_bare_options_object_has_working_defaults()
    {
        var o = new AgilityManagementOptions();

        Assert.Equal(TimeSpan.FromSeconds(3), o.BatchPolling.Interval);
        Assert.Equal(TimeSpan.FromMinutes(15), o.BatchPolling.Timeout);
        Assert.Equal(TimeSpan.FromSeconds(30), o.BatchPolling.NotFoundGracePeriod);
        Assert.Equal(3, o.Retry.MaxRetries);
        Assert.Equal(TimeSpan.FromMilliseconds(500), o.Retry.BaseDelay);
        Assert.Equal(TimeSpan.FromSeconds(30), o.Retry.MaxDelay);
        Assert.Null(o.BaseUrl);
    }

    [Fact]
    public async Task Without_credentials_the_OAuth_and_anonymous_endpoints_still_work_but_others_explain_what_is_missing()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        using var client = new AgilityManagementClient(new AgilityManagementOptions(), new HttpClient(handler));
        var ct = TestContext.Current.CancellationToken;

        await client.Types.GetAllTypesAsync(ct);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.ServerUsers.GetCurrentUserAsync(ct));

        Assert.Contains("AccessToken", ex.Message, StringComparison.Ordinal);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_refresh_token_option_gets_access_tokens_from_the_OAuth_refresh_endpoint()
    {
        var handler = new FakeHandler(r => r.Uri.AbsolutePath == "/oauth/refresh"
            ? FakeHandler.Json("""{"access_token":"fresh","expires_in":3600,"refresh_token":"rotated"}""")
            : FakeHandler.Json("{}"));
        string? stored = null;
        using var client = new AgilityManagementClient(
            new AgilityManagementOptions { RefreshToken = "original", RefreshTokenChanged = t => stored = t }, new HttpClient(handler));

        await client.ServerUsers.GetCurrentUserAsync(TestContext.Current.CancellationToken);

        Assert.Equal("refresh_token=original", handler.Requests[0].Body);
        Assert.Equal("Bearer fresh", handler.Requests[1].Headers.Authorization);
        Assert.Equal("rotated", stored);
    }

    [Fact]
    public void Nonsensical_timings_are_rejected()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new AgilityManagementClient(new AgilityManagementOptions { AccessToken = "t", BatchPolling = { Interval = TimeSpan.Zero } }));
        Assert.Throws<InvalidOperationException>(() =>
            new AgilityManagementClient(new AgilityManagementOptions { AccessToken = "t", Retry = { MaxRetries = -1 } }));
    }
}

public class DependencyInjectionTests
{
    [Fact]
    public async Task AddAgilityManagement_resolves_a_client_that_uses_the_configured_handler()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        var services = new ServiceCollection();
        services.AddAgilityManagement(o => o.AccessToken = "di-token")
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<AgilityManagementClient>();
        await client.Models.GetFieldTypesAsync(TestClient.InstanceGuid, TestContext.Current.CancellationToken);

        Assert.Equal("Bearer di-token", handler.Requests.Single().Headers.Authorization);
    }

    [Fact]
    public async Task A_registered_token_provider_is_used_when_the_options_set_none()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("[]"));
        var services = new ServiceCollection();
        services.AddSingleton<IAccessTokenProvider>(new FixedProvider("from-di"));
        services.AddAgilityManagement(_ => { }).ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<AgilityManagementClient>()
            .Models.GetFieldTypesAsync(TestClient.InstanceGuid, TestContext.Current.CancellationToken);

        Assert.Equal("Bearer from-di", handler.Requests.Single().Headers.Authorization);
    }

    private sealed class FixedProvider(string token) : IAccessTokenProvider
    {
        public ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken) => ValueTask.FromResult(token);
    }
}

public class RefreshTokenProviderTests
{
    [Fact]
    public async Task Caches_the_access_token_until_shortly_before_it_expires()
    {
        var time = new ManualTime(DateTimeOffset.Parse("2026-01-01T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture));
        var calls = 0;
        using var provider = new RefreshTokenAccessTokenProvider((_, _) =>
            Task.FromResult(new TokenResponseData { AccessToken = $"a{++calls}", ExpiresIn = 600 }), "r0", time);
        var ct = TestContext.Current.CancellationToken;

        Assert.Equal("a1", await provider.GetAccessTokenAsync(ct));
        time.Advance(TimeSpan.FromMinutes(7));
        Assert.Equal("a1", await provider.GetAccessTokenAsync(ct));
        time.Advance(TimeSpan.FromMinutes(2)); // within the 2-minute refresh margin
        Assert.Equal("a2", await provider.GetAccessTokenAsync(ct));
    }

    [Fact]
    public async Task Reports_a_rotated_refresh_token_and_uses_it_next_time()
    {
        var time = new ManualTime(DateTimeOffset.UnixEpoch);
        var seen = new List<string>();
        using var provider = new RefreshTokenAccessTokenProvider((refresh, _) =>
        {
            seen.Add(refresh);
            return Task.FromResult(new TokenResponseData { AccessToken = "a", ExpiresIn = 60, RefreshToken = refresh + "+" });
        }, "r", time);
        var changes = new List<string>();
        provider.RefreshTokenChanged += (_, token) => changes.Add(token);

        await provider.GetAccessTokenAsync(TestContext.Current.CancellationToken);
        time.Advance(TimeSpan.FromMinutes(5));
        await provider.GetAccessTokenAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["r", "r+"], seen);
        Assert.Equal(["r+", "r++"], changes);
    }

    private sealed class ManualTime(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
