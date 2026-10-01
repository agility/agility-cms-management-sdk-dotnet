using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// The OAuth endpoints, for applications that sign users in with their Agility account. For scripts
/// and CI, a Personal Access Token is simpler; see <see cref="AgilityManagementOptions.AccessToken"/>.
/// </summary>
public sealed class OAuthClient
{
    private readonly ManagementConnection _connection;

    internal OAuthClient(ManagementConnection connection) => _connection = connection;

    /// <summary>
    /// Builds the URL to send the user's browser to for sign-in. <c>GET /oauth/authorize</c>. After sign-in,
    /// the browser is redirected to <paramref name="redirectUri"/> with a <c>code</c> to pass to
    /// <see cref="ExchangeCodeAsync"/>.
    /// </summary>
    /// <param name="redirectUri">Where the API sends the browser after sign-in.</param>
    /// <param name="state">An opaque value returned to the redirect URI, to guard against CSRF.</param>
    /// <param name="offlineAccess">Ask for a refresh token as well as an access token (scope <c>offline_access</c>).</param>
    public Uri GetAuthorizeUri(Uri redirectUri, string? state = null, bool offlineAccess = true)
    {
        ArgumentNullException.ThrowIfNull(redirectUri);
        return _connection.RootUri($"oauth/authorize", new Query()
            .Add("response_type", "code")
            .Add("redirect_uri", redirectUri.AbsoluteUri)
            .Add("state", state)
            .Add("scope", offlineAccess ? "offline_access" : null));
    }

    /// <summary>Gets an instance's Fetch API key for published content. <c>GET /oauth/getfetchkey</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<string> GetFetchApiKeyAsync(string instanceGuid, CancellationToken cancellationToken = default) =>
        GetKeyAsync(instanceGuid, "getfetchkey", cancellationToken);

    /// <summary>Gets an instance's Fetch API key for preview (staging) content. <c>GET /oauth/getpreviewkey</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<string> GetPreviewApiKeyAsync(string instanceGuid, CancellationToken cancellationToken = default) =>
        GetKeyAsync(instanceGuid, "getpreviewkey", cancellationToken);

    // Keys are served by the instance's regional host.
    private Task<string> GetKeyAsync(string instanceGuid, string route, CancellationToken cancellationToken) =>
        _connection.SendForStringAsync(HttpMethod.Get,
            _connection.InstanceRootUri(instanceGuid, $"oauth/{route}", new Query().Add("guid", instanceGuid)),
            RequestKind.Read, cancellationToken: cancellationToken);

    /// <summary>Exchanges the authorization code from the sign-in redirect for tokens. <c>POST /oauth/token</c></summary>
    /// <param name="code">The <c>code</c> query value the browser brought back.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<TokenResponseData> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.RootUri($"oauth/token"), RequestKind.Write,
            ManagementJsonContext.Default.TokenResponseData,
            () => new FormUrlEncodedContent([new("code", code)]), anonymous: true, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a new access token from a refresh token. <c>POST /oauth/refresh</c>. The token is sent in the
    /// request body, not the URL, so it stays out of logs. <see cref="RefreshTokenAccessTokenProvider"/>
    /// calls this for you.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<TokenResponseData> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.RootUri($"oauth/refresh"), RequestKind.Write,
            ManagementJsonContext.Default.TokenResponseData,
            () => new FormUrlEncodedContent([new("refresh_token", refreshToken)]), anonymous: true, cancellationToken: cancellationToken);
    }
}
