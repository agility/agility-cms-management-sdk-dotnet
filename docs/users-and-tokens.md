# Users and tokens

## The signed-in user

```csharp
ServerUser me = await client.Users.GetCurrentUserAsync();
foreach (var site in me.WebsiteAccess ?? [])
    Console.WriteLine($"{site.WebsiteName}: {site.Guid}");
```

This is handy for finding the instance GUIDs a token can reach.

## Instance users

`instance.Users` manages who can use an instance. These endpoints need an [OAuth token](authentication.md#oauth);
Personal Access Tokens are refused.

```csharp
List<WebsiteUser> users = await instance.Users.GetUsersAsync();

// Add a user, or replace an existing user's roles.
InstanceUser user = await instance.Users.SaveUserAsync("editor@example.com",
    [new InstanceRole { RoleID = editorRoleId }], firstName: "Sam", lastName: "Lee");

await instance.Users.DeleteUserAsync(user.UserID);
```

## Personal Access Tokens

`client.PersonalAccessTokens` manages your tokens. It needs an OAuth token: a PAT can't create, list or revoke
tokens.

```csharp
var created = await client.PersonalAccessTokens.CreateTokenAsync(new PersonalAccessTokenRequest
{
    Name = "nightly-sync",
    ExpiryDate = DateTime.UtcNow.AddYears(1),   // at most 2 years; that's also the default
});
StoreSecret(created.Token!);   // shown only once

PersonalAccessTokenListResponse mine = await client.PersonalAccessTokens.GetTokensAsync();   // no token values
await client.PersonalAccessTokens.UpdateTokenAsync(created.TokenID, new PersonalAccessTokenUpdateRequest { Enabled = false });
await client.PersonalAccessTokens.RevokeTokenAsync(created.TokenID);
```

The API limits each user to 10 active tokens and 5 creations an hour, and returns 429 beyond that.

## Fetch API keys

The Fetch API keys for an instance's published and preview content:

```csharp
string fetchKey = await instance.GetFetchApiKeyAsync();
string previewKey = await instance.GetPreviewApiKeyAsync();
```

## API enums

`client.Types.GetAllTypesAsync()` returns the API's enums (batch operation types, item states, and so on) with
their names and values. It needs no token.
