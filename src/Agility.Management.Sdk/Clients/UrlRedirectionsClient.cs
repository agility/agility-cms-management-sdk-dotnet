using System.Net.Http.Headers;
using Agility.Management.Sdk.Http;
using Agility.Management.Sdk.Models;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Clients;

/// <summary>URL redirections.</summary>
public sealed class UrlRedirectionsClient
{
    private readonly ManagementConnection _connection;
    private readonly string _guid;

    internal UrlRedirectionsClient(ManagementConnection connection, string guid)
    {
        _connection = connection;
        _guid = guid;
    }

    /// <summary>Creates or updates redirections. <c>POST url-redirections</c></summary>
    /// <param name="redirections">The redirections. Use a <c>UrlRedirectionID</c> of 0 to create one.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<UrlRedirectionSaveResult> SaveUrlRedirectionsAsync(IReadOnlyList<UrlRedirection> redirections, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(redirections);
        var list = redirections as List<UrlRedirection> ?? [.. redirections];
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"url-redirections"),
            RequestKind.Write, ManagementJsonContext.Default.UrlRedirectionSaveResult,
            ManagementConnection.Json(list, ManagementJsonContext.Default.ListUrlRedirection), cancellationToken: cancellationToken);
    }

    /// <summary>Deletes redirections. <c>DELETE url-redirections?ids=</c></summary>
    /// <param name="redirectionIds">The redirection IDs.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<UrlRedirectionDeleteResult> DeleteUrlRedirectionsAsync(IEnumerable<int> redirectionIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(redirectionIds);
        var ids = redirectionIds.ToList();
        if (ids.Count == 0) throw new ArgumentException("Pass at least one redirection ID.", nameof(redirectionIds));
        return _connection.SendRequiredAsync(HttpMethod.Delete,
            _connection.InstanceUri(_guid, $"url-redirections", new Query().Add("ids", ids)),
            RequestKind.Write, ManagementJsonContext.Default.UrlRedirectionDeleteResult, cancellationToken: cancellationToken);
    }

    /// <summary>Imports redirections from a spreadsheet. <c>POST url-redirections/import</c></summary>
    /// <param name="file">The spreadsheet, in the format <see cref="ExportUrlRedirectionsAsync"/> produces. The SDK doesn't dispose it.</param>
    /// <param name="fileName">The file name to send, e.g. <c>redirections.xlsx</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    public Task<UrlRedirectionSaveResult> ImportUrlRedirectionsAsync(Stream file, string fileName = "redirections.xlsx", CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        HttpContent Build()
        {
            if (file.CanSeek) file.Position = 0;
            var part = new StreamContent(new NonDisposingStream(file));
            part.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return new MultipartFormDataContent { { part, "file", fileName } };
        }
        return _connection.SendRequiredAsync(HttpMethod.Post, _connection.InstanceUri(_guid, $"url-redirections/import"),
            RequestKind.Write, ManagementJsonContext.Default.UrlRedirectionSaveResult, Build, cancellationToken: cancellationToken);
    }

    /// <summary>Exports every redirection as an Excel spreadsheet. <c>GET url-redirections/export</c></summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The <c>.xlsx</c> file. Dispose it when you're done.</returns>
    public Task<Stream> ExportUrlRedirectionsAsync(CancellationToken cancellationToken = default) =>
        _connection.SendForStreamAsync(HttpMethod.Get, _connection.InstanceUri(_guid, $"url-redirections/export"),
            RequestKind.Read, cancellationToken);
}
