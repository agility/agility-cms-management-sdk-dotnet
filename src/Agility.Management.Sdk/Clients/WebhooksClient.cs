using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>Webhooks, their delivery history and their signing secrets.</summary>
public sealed class WebhooksClient
{
    private readonly ManagementConnection _connection;

    internal WebhooksClient(ManagementConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Lists webhooks a page at a time. <c>GET webhook/list</c>. Pass the returned <c>Token</c> back as
    /// <paramref name="continuationToken"/> for the next page; it's <see langword="null"/> after the last page.
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="take">Webhooks per page.</param>
    /// <param name="continuationToken">The token from the previous page.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<WebhookContinuationListing> GetWebhooksAsync(string instanceGuid, int? take = null, string? continuationToken = null, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"webhook/list", new Query().Add("take", take).Add("token", continuationToken)),
            RequestKind.Read, ManagementJsonContext.Default.WebhookContinuationListing, cancellationToken: cancellationToken);

    /// <summary>Gets a webhook. <c>GET webhook/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="webhookId">The webhook ID (its <c>RowKey</c>).</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Webhook> GetWebhookAsync(string instanceGuid, string webhookId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookId);
        return _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(instanceGuid, $"webhook/{webhookId}"),
            RequestKind.Read, ManagementJsonContext.Default.Webhook, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates or updates a webhook. <c>POST webhook</c>. With <c>SecureDeliveryEnabled</c>, the first save
    /// creates a signing secret; it's returned in full only in that response (see <c>SigningSecretJustCreated</c>).
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="webhook">The webhook. Leave <c>RowKey</c> empty to create one.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Webhook> SaveWebhookAsync(string instanceGuid, Webhook webhook, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(webhook);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(instanceGuid, $"webhook"),
            RequestKind.Write, ManagementJsonContext.Default.Webhook,
            ManagementConnection.Json(webhook, ManagementJsonContext.Default.Webhook), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes a webhook. <c>DELETE webhook/{id}</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task DeleteWebhookAsync(string instanceGuid, string webhookId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookId);
        return _connection.SendAsync(HttpMethod.Delete, _connection.InstanceUri(instanceGuid, $"webhook/{webhookId}"),
            RequestKind.Write, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a new signing secret. <c>POST webhook/{id}/rotate-secret</c>. The previous secret keeps signing
    /// deliveries for 24 hours so receivers can switch over.
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Webhook> RotateSigningSecretAsync(string instanceGuid, string webhookId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookId);
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(instanceGuid, $"webhook/{webhookId}/rotate-secret"),
            RequestKind.Write, ManagementJsonContext.Default.Webhook, cancellationToken: cancellationToken);
    }

    /// <summary>Lists a webhook's deliveries a page at a time. <c>GET webhook/{id}/history</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="webhookId">The webhook ID.</param>
    /// <param name="fromDate">Only deliveries after this time.</param>
    /// <param name="toDate">Only deliveries before this time.</param>
    /// <param name="take">Deliveries per page.</param>
    /// <param name="continuationToken">The token from the previous page.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<WebhookHistoryContinuationListing> GetWebhookHistoryAsync(string instanceGuid, string webhookId, DateTime? fromDate = null, DateTime? toDate = null, int? take = null, string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(webhookId);
        return _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"webhook/{webhookId}/history", new Query()
                .Add("fromDate", fromDate).Add("toDate", toDate).Add("take", take).Add("token", continuationToken)),
            RequestKind.Read, ManagementJsonContext.Default.WebhookHistoryContinuationListing, cancellationToken: cancellationToken);
    }
}
