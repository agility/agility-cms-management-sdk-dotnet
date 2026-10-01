using System.Net;
using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk;

/// <summary>The Management API returned an error response.</summary>
public class AgilityManagementException : Exception
{
    /// <summary>Creates the exception.</summary>
    public AgilityManagementException(string message, Exception? innerException = null)
        : base(message, innerException) { }

    /// <summary>The HTTP status code, when the error came from a response.</summary>
    public HttpStatusCode? StatusCode { get; init; }

    /// <summary>The request method.</summary>
    public string? Method { get; init; }

    /// <summary>The request URI. Query values are kept, so don't log it if they're sensitive.</summary>
    public Uri? RequestUri { get; init; }

    /// <summary>The response body, trimmed to 16 KB.</summary>
    public string? ResponseBody { get; init; }

    /// <summary>The error message the API sent, when the body contained one.</summary>
    public string? ApiMessage { get; init; }

    /// <summary>The parsed body, when it was an RFC 7807 problem response.</summary>
    public ProblemDetails? Problem { get; init; }

    /// <summary>A request or trace ID from the response, to quote when reporting a problem.</summary>
    public string? RequestId { get; init; }
}

/// <summary>A batch finished, but the API reported that it or some of its items failed.</summary>
public class AgilityBatchException : AgilityManagementException
{
    /// <summary>Creates the exception.</summary>
    public AgilityBatchException(string message, int batchId, Batch? batch, Exception? innerException = null)
        : base(message, innerException)
    {
        BatchId = batchId;
        Batch = batch;
    }

    /// <summary>The batch ID.</summary>
    public int BatchId { get; }

    /// <summary>The last state of the batch the SDK saw, including any items that succeeded.</summary>
    public Batch? Batch { get; }
}

/// <summary>
/// A batch didn't finish within <see cref="BatchPollingOptions.Timeout"/>. The batch keeps running on the
/// server; check it later with <see cref="Clients.BatchesClient.GetBatchAsync"/>.
/// </summary>
public sealed class AgilityBatchTimeoutException : AgilityBatchException
{
    /// <summary>Creates the exception.</summary>
    public AgilityBatchTimeoutException(string message, int batchId, Batch? batch, TimeSpan waited, Exception? innerException = null)
        : base(message, batchId, batch, innerException)
    {
        Waited = waited;
    }

    /// <summary>How long the SDK waited.</summary>
    public TimeSpan Waited { get; }
}
