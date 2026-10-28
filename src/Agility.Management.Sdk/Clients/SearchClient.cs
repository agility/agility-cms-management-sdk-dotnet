using System.Net;
using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>
/// Search across an instance: content items, components, pages, assets, models and containers.
/// </summary>
public sealed class SearchClient
{
    private readonly ManagementConnection _connection;

    internal SearchClient(ManagementConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Searches everything in an instance and returns one page of results, best match first, with a count per kind.
    /// <c>POST search</c>
    /// </summary>
    /// <remarks>
    /// Words match as prefixes ("newsl" finds "newsletter") and every word must match; text in double quotes is an
    /// exact phrase, and a number also matches IDs exactly. When nothing matches every word, the results match any
    /// of them and <see cref="SearchResult.Relaxed"/> is <see langword="true"/>. Results only include what the
    /// caller is allowed to see. Changes are searchable a few seconds after they're saved; after a large import
    /// <see cref="SearchResult.Indexing"/> says how far the index is behind.
    /// </remarks>
    /// <param name="instanceGuid">The instance GUID, e.g. <c>1234abcd-u</c>. Its suffix selects the region.</param>
    /// <param name="query">The text to search for.</param>
    /// <param name="options">Filters and paging. <see langword="null"/>: every kind and locale, the first 25 results.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="AgilitySearchNotAvailableException">Search isn't available for this instance yet (501).</exception>
    /// <exception cref="AgilityManagementException">The API refused the search, e.g. 400 for an unknown kind or state.</exception>
    public async Task<SearchResult> SearchAsync(string instanceGuid, string query, SearchOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        var o = options ?? new SearchOptions();
        var request = new SearchRequest
        {
            Query = query,
            Kinds = o.Kinds is null ? null : [.. o.Kinds],
            Locale = o.Locale,
            Containers = o.Containers is null ? null : [.. o.Containers],
            Models = o.Models is null ? null : [.. o.Models],
            State = o.State,
            ModifiedAfter = o.ModifiedAfter,
            // The spec's skip and take aren't nullable, so they're always sent: use the API's defaults when unset.
            Skip = o.Skip ?? 0,
            Take = o.Take ?? SearchOptions.DefaultTake,
        };
        try
        {
            return await _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(instanceGuid, $"search"),
                RequestKind.Read, ManagementJsonContext.Default.SearchResult,
                ManagementConnection.Json(request, ManagementJsonContext.Default.SearchRequest), cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (AgilityManagementException ex) when (ex.StatusCode == HttpStatusCode.NotImplemented)
        {
            throw new AgilitySearchNotAvailableException(ex);
        }
    }
}
