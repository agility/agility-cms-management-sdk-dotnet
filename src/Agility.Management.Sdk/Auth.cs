using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk;

/// <summary>Supplies the bearer token for Management API requests.</summary>
public interface IAccessTokenProvider
{
    /// <summary>
    /// Returns a valid access token. Called before every request, so implementations should cache
    /// the token and refresh it only when it's about to expire.
    /// </summary>
    ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken);
}

internal sealed class StaticAccessTokenProvider(string token) : IAccessTokenProvider
{
    public ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken) => ValueTask.FromResult(token);
}

/// <summary>
/// Gets access tokens from an OAuth refresh token and renews them shortly before they expire.
/// The API may rotate the refresh token on each refresh; subscribe to <see cref="RefreshTokenChanged"/>
/// to store the new one.
/// </summary>
/// <remarks>
/// For scripts and CI a Personal Access Token set on <see cref="AgilityManagementOptions.AccessToken"/>
/// is simpler: it doesn't expire until the date you choose.
/// </remarks>
public sealed class RefreshTokenAccessTokenProvider : IAccessTokenProvider, IDisposable
{
    private static readonly TimeSpan MaxRefreshMargin = TimeSpan.FromMinutes(2);

    private readonly Func<string, CancellationToken, Task<TokenResponseData>> _refresh;
    private readonly TimeProvider _time;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string _refreshToken;
    // Token and refresh time are read together without the lock, so they're swapped as one object.
    private volatile CachedToken? _cached;

    /// <summary>Creates a provider that refreshes against <c>/oauth/refresh</c> on <paramref name="oauth"/>.</summary>
    public RefreshTokenAccessTokenProvider(Clients.OAuthClient oauth, string refreshToken, TimeProvider? timeProvider = null)
        : this((token, ct) => oauth.RefreshAsync(token, ct), refreshToken, timeProvider)
    {
        ArgumentNullException.ThrowIfNull(oauth);
    }

    internal RefreshTokenAccessTokenProvider(
        Func<string, CancellationToken, Task<TokenResponseData>> refresh, string refreshToken, TimeProvider? timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        _refresh = refresh;
        _refreshToken = refreshToken;
        _time = timeProvider ?? TimeProvider.System;
    }

    /// <summary>Raised with the new refresh token when the API issues one.</summary>
    public event EventHandler<string>? RefreshTokenChanged;

    /// <inheritdoc/>
    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (_cached is { } cached && _time.GetUtcNow() < cached.RefreshAt)
            return cached.Token;

        string? rotated = null;
        string token;
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cached is { } again && _time.GetUtcNow() < again.RefreshAt)
                return again.Token;

            var response = await _refresh(_refreshToken, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(response.AccessToken))
                throw new AgilityManagementException("The OAuth refresh response had no access token.");

            // No expiry in the response: assume a short lifetime rather than caching forever. Refresh a little
            // early, but never so early that a short-lived token is refreshed on every request.
            var lifetime = TimeSpan.FromSeconds(response.ExpiresIn is > 0 ? response.ExpiresIn.Value : 300);
            var margin = lifetime / 2 < MaxRefreshMargin ? lifetime / 2 : MaxRefreshMargin;
            token = response.AccessToken;
            _cached = new CachedToken(token, _time.GetUtcNow() + lifetime - margin);

            if (!string.IsNullOrEmpty(response.RefreshToken) && response.RefreshToken != _refreshToken)
                rotated = _refreshToken = response.RefreshToken;
        }
        finally
        {
            _lock.Release();
        }

        // Outside the lock, so a handler can call back into the provider.
        if (rotated is not null) RefreshTokenChanged?.Invoke(this, rotated);
        return token;
    }

    /// <inheritdoc/>
    public void Dispose() => _lock.Dispose();

    private sealed record CachedToken(string Token, DateTimeOffset RefreshAt);
}
