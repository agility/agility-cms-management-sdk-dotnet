using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>The instance's locales.</summary>
public sealed class LocalesClient
{
    private readonly ManagementConnection _connection;
    private readonly string _guid;

    internal LocalesClient(ManagementConnection connection, string guid)
    {
        _connection = connection;
        _guid = guid;
    }

    /// <summary>Lists the instance's enabled locales. <c>GET locales</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<List<Locale>> GetLocalesAsync(CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"locales"),
            RequestKind.Read, ManagementJsonContext.Default.ListLocale, cancellationToken: cancellationToken);

    /// <summary>Lists every locale, enabled or not. <c>GET locales/all</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<LocalesResponse> GetAllLocalesAsync(CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"locales/all"),
            RequestKind.Read, ManagementJsonContext.Default.LocalesResponse, cancellationToken: cancellationToken);

    /// <summary>Gets a locale by ID. <c>GET locales/{localeId}</c></summary>
    /// <param name="localeId">The locale ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Locale> GetLocaleAsync(int localeId, CancellationToken cancellationToken = default) =>
        _connection.SendRequiredAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"locales/{localeId}"),
            RequestKind.Read, ManagementJsonContext.Default.Locale, cancellationToken: cancellationToken);

    /// <summary>
    /// Creates or updates a locale. <c>POST locales</c>. Returns the saved locale, or <see langword="null"/> if
    /// the API sent no body.
    /// </summary>
    /// <param name="locale">The locale. <c>LocaleName</c> and <c>LocaleCode</c> are required.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Locale?> SaveLocaleAsync(Locale locale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(locale);
        return _connection.SendAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"locales"),
            RequestKind.Write, ManagementJsonContext.Default.Locale,
            ManagementConnection.Json(locale, ManagementJsonContext.Default.Locale), cancellationToken: cancellationToken);
    }

    /// <summary>Enables a locale. <c>PATCH locales/{localeId}/enable</c></summary>
    /// <param name="localeId">The locale ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Locale?> EnableLocaleAsync(int localeId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Patch, _connection.InstanceUri(_guid, $"locales/{localeId}/enable"),
            RequestKind.Write, ManagementJsonContext.Default.Locale, cancellationToken: cancellationToken);

    /// <summary>Disables a locale. <c>PATCH locales/{localeId}/disable</c></summary>
    /// <param name="localeId">The locale ID.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<Locale?> DisableLocaleAsync(int localeId, CancellationToken cancellationToken = default) =>
        _connection.SendAsync(HttpMethod.Patch, _connection.InstanceUri(_guid, $"locales/{localeId}/disable"),
            RequestKind.Write, ManagementJsonContext.Default.Locale, cancellationToken: cancellationToken);

    /// <summary>Sets the order locales appear in. <c>POST locales/sort-order</c></summary>
    /// <param name="orderedLocaleIds">Every locale ID, in the order you want.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task SetSortOrderAsync(IEnumerable<int> orderedLocaleIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderedLocaleIds);
        var body = new SortOrder { OrderedIds = [.. orderedLocaleIds] };
        if (body.OrderedIds.Count == 0) throw new ArgumentException("Pass at least one locale ID.", nameof(orderedLocaleIds));
        return _connection.SendAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"locales/sort-order"),
            RequestKind.Write, ManagementConnection.Json(body, ManagementJsonContext.Default.SortOrder), cancellationToken: cancellationToken);
    }
}
