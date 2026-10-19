using System.Text.Json;
using System.Text.Json.Serialization;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Models;

// A page's whole `geo` block has the same three states on save as each of its properties: left out (Geo null here)
// keeps what's stored, an explicit JSON null clears all of it, and a value is merged property by property. The SDK
// never sends null for an unset property, so clearing the block goes through ClearGeo().

public sealed partial class PageItem : IJsonOnSerializing, IJsonOnDeserialized
{
    // Holds only the explicit nulls requested with ClearGeo(). It's internal and emptied after every read, so
    // unlike SeoProperties/GeoProperties, members of a page the SDK doesn't know are still dropped, not sent back.
    [JsonExtensionData]
    [JsonInclude]
    internal Dictionary<string, JsonElement>? ExplicitNullData { get; set; }

    /// <summary>
    /// Clears the page's whole stored <c>geo</c> block on save, by sending <c>"geo": null</c>.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="Geo"/> to <see langword="null"/>. Setting <see cref="Geo"/> to a value afterwards cancels the
    /// clear: the value is sent instead, and the API merges it with what's stored. A <see cref="Geo"/> that's simply
    /// <see langword="null"/>, without this call, is left out of the request and keeps what's stored.
    /// </remarks>
    /// <returns>This page, for chaining.</returns>
    public PageItem ClearGeo() { Geo = null; ExplicitNullData = ExplicitNulls.Add(ExplicitNullData, "geo"); return this; }

    void IJsonOnSerializing.OnSerializing() => ExplicitNulls.DropSuperseded(ExplicitNullData, ("geo", Geo is not null));

    void IJsonOnDeserialized.OnDeserialized() => ExplicitNullData = null;
}
