# Webhooks

`client.Webhooks` manages the instance's webhooks.

## Create and update

```csharp
var webhook = await client.Webhooks.SaveWebhookAsync(guid, new Webhook
{
    Name = "Rebuild site",
    Url = "https://build.example.com/hooks/agility",
    Enabled = true,
    ContentPublishEvents = true,
    ContentSaveEvents = false,
    ContentWorkflowEvents = false,
    SecureDeliveryEnabled = true,   // sign deliveries (Standard Webhooks)
});

if (webhook.SigningSecretJustCreated)
    StoreSecret(webhook.SigningSecret!);   // returned in full only now
```

A webhook's ID is its `RowKey`. To update one, get it, change it and save it.

## Signed delivery

With `SecureDeliveryEnabled`, the API signs every delivery following the
[Standard Webhooks](https://www.standardwebhooks.com/) spec, using the webhook's signing secret (`whsec_...`).
Verify the signature in your receiver with a Standard Webhooks library.

Rotate the secret with `RotateSigningSecretAsync`. The previous secret keeps signing for 24 hours
(`PreviousSigningSecret`), so receivers can switch over without dropping deliveries.

```csharp
var rotated = await client.Webhooks.RotateSigningSecretAsync(guid, webhook.RowKey!);
StoreSecret(rotated.SigningSecret!);
```

## List, inspect and delete

Webhook lists and delivery history are paged with a continuation token:

```csharp
string? token = null;
do
{
    var page = await client.Webhooks.GetWebhooksAsync(guid, take: 50, continuationToken: token);
    foreach (var hook in page.Items ?? []) Console.WriteLine(hook.Name);
    token = page.Token;
}
while (token is not null);

var history = await client.Webhooks.GetWebhookHistoryAsync(guid, webhook.RowKey!, fromDate: DateTime.UtcNow.AddDays(-1));
await client.Webhooks.DeleteWebhookAsync(guid, webhook.RowKey!);
```
