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
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(2);

    private readonly Func<string, CancellationToken, Task<TokenResponseData>> _refresh;
    private readonly TimeProvider _time;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string _refreshToken;
    private string? _accessToken;
    private DateTimeOffset _expiresAt;

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
        if (_accessToken is { } cached && _time.GetUtcNow() < _expiresAt - RefreshMargin)
            return cached;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_accessToken is { } again && _time.GetUtcNow() < _expiresAt - RefreshMargin)
                return again;

            var response = await _refresh(_refreshToken, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrEmpty(response.AccessToken))
                throw new AgilityManagementException("The OAuth refresh response had no access token.");

            _accessToken = response.AccessToken;
            // No expiry in the response: assume a short lifetime rather than caching forever.
            _expiresAt = _time.GetUtcNow() + TimeSpan.FromSeconds(response.ExpiresIn ?? 300);
            if (!string.IsNullOrEmpty(response.RefreshToken) && response.RefreshToken != _refreshToken)
            {
                _refreshToken = response.RefreshToken;
                RefreshTokenChanged?.Invoke(this, _refreshToken);
            }
            return _accessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _lock.Dispose();
}
