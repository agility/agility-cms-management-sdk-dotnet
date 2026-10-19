using System.Text.Json;
using System.Text.Json.Serialization;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Models;

// Title, CanonicalUrl, Robots and OgImage (pages only) have three states on save: left out (null here) keeps what's
// stored, an explicit JSON null clears it, and a value replaces it. The SDK never sends null for an unset
// property, so clearing goes through the Clear* methods below.

public sealed partial class SeoProperties : IJsonOnSerializing
{
    /// <summary>
    /// Any other members the API returns (kept and sent back on save), plus the clears requested with the
    /// <c>Clear*</c> methods.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    /// <summary>Clears the stored <c>title</c> on save, so the page's title is used.</summary>
    public SeoProperties ClearTitle() { Title = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "title"); return this; }

    /// <summary>Clears the stored <c>canonicalUrl</c> on save.</summary>
    public SeoProperties ClearCanonicalUrl() { CanonicalUrl = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "canonicalUrl"); return this; }

    /// <summary>Clears the stored <c>robots</c> directives on save.</summary>
    public SeoProperties ClearRobots() { Robots = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "robots"); return this; }

    /// <summary>Clears the stored <c>ogImage</c> on save.</summary>
    public SeoProperties ClearOgImage() { OgImage = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "ogImage"); return this; }

    void IJsonOnSerializing.OnSerializing() => ExplicitNulls.DropSuperseded(AdditionalProperties,
        ("title", Title is not null), ("canonicalUrl", CanonicalUrl is not null),
        ("robots", Robots is not null), ("ogImage", OgImage is not null));
}
