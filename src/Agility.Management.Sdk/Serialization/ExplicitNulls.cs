using System.Text.Json;

namespace Agility.Management.Sdk.Serialization;

/// <summary>
/// Sends an explicit JSON <c>null</c> for a property, on models where the API reads <c>null</c> as "clear".
/// </summary>
/// <remarks>
/// The SDK leaves every <c>null</c> property out of requests, and the API keeps what's stored for a property it
/// isn't sent. A clear therefore travels in the model's extension data as <c>"name": null</c>, and is dropped
/// again if the typed property has been given a value since.
/// </remarks>
internal static class ExplicitNulls
{
    private static readonly JsonElement Null = JsonDocument.Parse("null").RootElement.Clone();

    public static Dictionary<string, JsonElement> Add(Dictionary<string, JsonElement>? extensionData, string wireName)
    {
        extensionData ??= new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        extensionData[wireName] = Null;
        return extensionData;
    }

    /// <summary>Removes each clear whose property now has a value, so the value wins and no key is sent twice.</summary>
    public static void DropSuperseded(Dictionary<string, JsonElement>? extensionData, params ReadOnlySpan<(string WireName, bool HasValue)> properties)
    {
        if (extensionData is null || extensionData.Count == 0) return;
        foreach (var (wireName, hasValue) in properties)
            if (hasValue && extensionData.TryGetValue(wireName, out var v) && v.ValueKind == JsonValueKind.Null)
                extensionData.Remove(wireName);
    }
}
