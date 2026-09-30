namespace Agility.Management.Sdk;

/// <summary>
/// Maps an instance GUID to the Management API host for its region. The region is the suffix after
/// the last dash: <c>1234abcd-u</c> is USA, <c>1234abcd-us2</c> is USA2, and so on. A GUID with no dash
/// is a USA instance.
/// </summary>
public static class AgilityRegions
{
    /// <summary>The host for server-level calls (tokens, users, OAuth) when no base URL is configured.</summary>
    public static Uri DefaultServerUrl { get; } = new("https://mgmt.aglty.io");

    /// <summary>Region suffix → Management API host.</summary>
    public static IReadOnlyDictionary<string, Uri> Hosts { get; } = new Dictionary<string, Uri>(StringComparer.OrdinalIgnoreCase)
    {
        ["u"] = new("https://mgmt.aglty.io"),
        ["us2"] = new("https://mgmt-usa2.aglty.io"),
        ["c"] = new("https://mgmt-ca.aglty.io"),
        ["e"] = new("https://mgmt-eu.aglty.io"),
        ["a"] = new("https://mgmt-aus.aglty.io"),
        ["d"] = new("https://mgmt-dev.aglty.io"),
    };

    /// <summary>Returns the Management API host that serves <paramref name="instanceGuid"/>.</summary>
    /// <exception cref="ArgumentException">
    /// The GUID ends in a suffix that isn't a known region. It is an error rather than a fallback to USA,
    /// because sending a request to the wrong region fails in confusing ways.
    /// </exception>
    public static Uri ResolveBaseUrl(string instanceGuid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceGuid);
        var dash = instanceGuid.LastIndexOf('-');
        if (dash < 0) return Hosts["u"];

        var suffix = instanceGuid[(dash + 1)..];
        if (Hosts.TryGetValue(suffix, out var host)) return host;

        throw new ArgumentException(
            $"Instance GUID '{instanceGuid}' ends in '-{suffix}', which isn't a known region ({string.Join(", ", Hosts.Keys)}). " +
            $"Check the GUID, or set {nameof(AgilityManagementOptions)}.{nameof(AgilityManagementOptions.BaseUrl)}.",
            nameof(instanceGuid));
    }
}
