using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// Whether published changes have reached the Fetch API (the read API your website uses).
/// </summary>
public sealed class SyncStatusClient
{
    private readonly ManagementConnection _connection;

    internal SyncStatusClient(ManagementConnection connection)
    {
        _connection = connection;
    }

    /// <summary>Gets the Fetch API sync status. <c>GET fetch-api-status</c></summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="mode"><see cref="SyncMode.Fetch"/> for published content, <see cref="SyncMode.Preview"/> for staging.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<SyncStatusResponse> GetFetchApiStatusAsync(string instanceGuid, SyncMode mode = SyncMode.Fetch, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get,
            _connection.InstanceUri(instanceGuid, $"fetch-api-status", new Query().Add("mode", mode.ToApiValue())),
            RequestKind.Read, ManagementJsonContext.Default.SyncStatusResponse, cancellationToken: cancellationToken);

    /// <summary>
    /// Waits until the Fetch API has no sync in progress, for example after publishing, before reading the
    /// published content back. Checks every <see cref="BatchPollingOptions.Interval"/>.
    /// </summary>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="mode">Which Fetch API to wait for.</param>
    /// <param name="timeout">How long to wait. Default: <see cref="BatchPollingOptions.Timeout"/>.</param>
    /// <param name="cancellationToken">Stops waiting.</param>
    /// <exception cref="TimeoutException">A sync was still in progress after <paramref name="timeout"/>.</exception>
    public async Task<SyncStatusResponse> WaitForFetchApiSyncAsync(string instanceGuid, SyncMode mode = SyncMode.Fetch, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var polling = _connection.Options.BatchPolling;
        var limit = timeout ?? polling.Timeout;
        var started = _connection.Time.GetTimestamp();
        while (true)
        {
            var status = await GetFetchApiStatusAsync(instanceGuid, mode, cancellationToken).ConfigureAwait(false);
            if (!status.InProgress) return status;

            var elapsed = _connection.Time.GetElapsedTime(started);
            if (elapsed >= limit)
                throw new TimeoutException($"The {mode} Fetch API was still syncing after {limit}.");
            var wait = polling.Interval < limit - elapsed ? polling.Interval : limit - elapsed;
            await Task.Delay(wait, _connection.Time, cancellationToken).ConfigureAwait(false);
        }
    }
}
