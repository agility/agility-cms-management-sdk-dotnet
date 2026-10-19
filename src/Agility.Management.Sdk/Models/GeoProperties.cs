using System.Text.Json;
using System.Text.Json.Serialization;
using Agility.Management.Sdk.Serialization;

namespace Agility.Management.Sdk.Models;

// Saving GEO data has three states per property: left out (null here) keeps what's stored, an explicit JSON null
// clears it, and a value replaces it. The SDK never sends null for an unset property, so clearing goes through
// the Clear* methods below.

public sealed partial class GeoProperties : IJsonOnSerializing
{
    /// <summary>
    /// Any other members the API returns (kept and sent back on save), plus the clears requested with the
    /// <c>Clear*</c> methods.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    /// <summary>Clears the stored <c>schemaType</c> on save, so the page reverts to <c>WebPage</c>.</summary>
    public GeoProperties ClearSchemaType() { SchemaType = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "schemaType"); return this; }

    /// <summary>Clears the stored <c>summary</c> on save.</summary>
    public GeoProperties ClearSummary() { Summary = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "summary"); return this; }

    /// <summary>Clears the stored <c>structuredData</c> on save.</summary>
    public GeoProperties ClearStructuredData() { StructuredData = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "structuredData"); return this; }

    /// <summary>Clears all the stored <c>ai</c> settings on save, so they revert to <c>inherit</c>.</summary>
    public GeoProperties ClearAi() { Ai = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "ai"); return this; }

    void IJsonOnSerializing.OnSerializing() => ExplicitNulls.DropSuperseded(AdditionalProperties,
        ("schemaType", SchemaType is not null), ("summary", Summary is not null),
        ("structuredData", StructuredData is not null), ("ai", Ai is not null));
}

public sealed partial class GeoAi : IJsonOnSerializing
{
    /// <summary>
    /// Any other members the API returns (kept and sent back on save), plus the clears requested with the
    /// <c>Clear*</c> methods.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }

    /// <summary>Clears the stored <c>llmsInclude</c> on save, so it reverts to <c>inherit</c>.</summary>
    public GeoAi ClearLlmsInclude() { LlmsInclude = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "llmsInclude"); return this; }

    /// <summary>Clears the stored <c>crawl</c> on save, so it reverts to <c>inherit</c>.</summary>
    public GeoAi ClearCrawl() { Crawl = null; AdditionalProperties = ExplicitNulls.Add(AdditionalProperties, "crawl"); return this; }

    void IJsonOnSerializing.OnSerializing() => ExplicitNulls.DropSuperseded(AdditionalProperties,
        ("llmsInclude", LlmsInclude is not null), ("crawl", Crawl is not null));
}

/// <summary>The values <see cref="GeoProperties.SchemaType"/> accepts.</summary>
public static class GeoSchemaTypes
{
    /// <summary><c>WebPage</c>, the default.</summary>
    public const string WebPage = "WebPage";
    /// <summary><c>AboutPage</c></summary>
    public const string AboutPage = "AboutPage";
    /// <summary><c>ContactPage</c></summary>
    public const string ContactPage = "ContactPage";
    /// <summary><c>CollectionPage</c></summary>
    public const string CollectionPage = "CollectionPage";
    /// <summary><c>FAQPage</c></summary>
    public const string FAQPage = "FAQPage";
    /// <summary><c>ItemPage</c></summary>
    public const string ItemPage = "ItemPage";
    /// <summary><c>ProfilePage</c></summary>
    public const string ProfilePage = "ProfilePage";
}
