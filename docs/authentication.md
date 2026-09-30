# Authentication

The Management API accepts a bearer token: either a **Personal Access Token** or an **OAuth access token**.

| | Personal Access Token | OAuth |
|---|---|---|
| Best for | scripts, CI, server-side jobs | apps where a person signs in |
| Lifetime | until the expiry you choose (up to 2 years) | short; renewed with a refresh token |
| Setup | create once, store as a secret | sign-in redirect, then refresh |
| Can manage users and tokens | no | yes |

## Personal Access Tokens

Create a token once (see [Personal Access Tokens](https://agilitycms.com/docs/developers/personal-access-tokens),
or `client.PersonalAccessTokens.CreateTokenAsync` from an OAuth-authenticated client), store it as a secret, and
pass it as `AccessToken`:

```csharp
using var client = new AgilityManagementClient(new AgilityManagementOptions
{
    AccessToken = Environment.GetEnvironmentVariable("AGILITY_TOKEN"),
});
```

PATs can't call these; use OAuth for them:

- instance user management (`instance.Users`)
- token management (`client.PersonalAccessTokens`)

## OAuth

1. Send the user's browser to the sign-in URL:

   ```csharp
   var signIn = client.OAuth.GetAuthorizeUri(new Uri("https://myapp.example.com/agility/callback"), state: csrfToken);
   ```

2. Agility redirects back to your URL with `?code=...&state=...`. Check `state`, then exchange the code:

   ```csharp
   TokenResponseData tokens = await client.OAuth.ExchangeCodeAsync(code);
   // tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresIn
   ```

3. Store the refresh token securely, and give it to the client. The client gets an access token from it and
   renews it two minutes before it expires:

   ```csharp
   using var client = new AgilityManagementClient(new AgilityManagementOptions
   {
       RefreshToken = storedRefreshToken,
       RefreshTokenChanged = newToken => SaveRefreshToken(newToken),   // if the API rotates it
   });
   ```

   Create one client and reuse it: each client built from a `RefreshToken` keeps its own token cache. With
   `AddAgilityManagement`, every client the container creates shares one, so the rotated refresh token reaches all
   of them.

   Steps 1 and 2 need no credentials, so the client you use for sign-in can be created with empty options.
   `RefreshTokenAccessTokenProvider` is the same logic as a standalone `IAccessTokenProvider`, if you'd rather
   share one provider between clients.

## Your own token provider

Implement `IAccessTokenProvider` to get tokens from anywhere, for example a secrets vault. It's called before
every request, so cache the token:

```csharp
public sealed class VaultTokenProvider(ISecretStore vault) : IAccessTokenProvider
{
    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken) =>
        await vault.GetCachedSecretAsync("agility-token", cancellationToken);
}
```

With dependency injection, register it and `AddAgilityManagement` picks it up when the options don't set one:

```csharp
services.AddSingleton<IAccessTokenProvider, VaultTokenProvider>();
services.AddAgilityManagement(_ => { });
```

## Keeping tokens safe

- Don't commit tokens; read them from environment variables or a secret store.
- The SDK sends the refresh token in the request body, not the URL, so it stays out of request logs.
- `AgilityManagementException.RequestUri` includes query values. Don't log it if your queries hold anything sensitive.
