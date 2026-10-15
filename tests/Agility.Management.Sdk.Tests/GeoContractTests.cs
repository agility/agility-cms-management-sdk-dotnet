using System.Text.Json.Nodes;
using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk.Tests;

/// <summary>
/// A page's <c>geo</c> and the newer <c>seo</c> properties have three states on save: left out keeps the stored
/// value, an explicit <c>null</c> clears it, and a value replaces it (PROD-2726). An unrelated save must never
/// clear them.
/// </summary>
public class GeoContractTests
{
    private const string PageWithGeo = """
        {
          "pageID": 7, "name": "about", "title": "About", "pageType": "static",
          "seo": { "metaDescription": "About us", "title": "About us | Example", "robots": { "index": true, "follow": false },
                   "ogImage": "https://cdn.example.com/share.png", "canonicalUrl": "https://example.com/about" },
          "geo": { "schemaType": "AboutPage", "summary": "Who we are.",
                   "structuredData": [{ "@type": "Organization", "name": "Example" }],
                   "ai": { "llmsInclude": "include", "crawl": "inherit" }, "futureKey": { "x": 1 } }
        }
        """;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static (AgilityManagementClient Client, FakeHandler Handler) Capture()
    {
        var handler = new FakeHandler(r => r.Method == HttpMethod.Get ? FakeHandler.Json(PageWithGeo) : FakeHandler.Json("9"));
        return (TestClient.Create(handler), handler);
    }

    private static JsonObject SentBody(FakeHandler handler) =>
        JsonNode.Parse(handler.Requests.Last().Body!)!.AsObject();

    private static Task<BatchResult> Save(AgilityManagementClient client, PageItem page) =>
        client.Pages.SavePageAsync(TestClient.InstanceGuid, "en-us", page, waitForBatch: false, cancellationToken: Ct);

    [Fact]
    public async Task A_page_without_geo_data_is_saved_without_geo_or_the_new_seo_properties()
    {
        var (client, handler) = Capture();

        await Save(client, new PageItem { PageID = 7, Title = "About", Seo = new SeoProperties { MetaDescription = "About us" } });

        var body = SentBody(handler);
        Assert.False(body.ContainsKey("geo"), body.ToJsonString());
        var seo = body["seo"]!.AsObject();
        foreach (var key in new[] { "title", "canonicalUrl", "robots", "ogImage" })
            Assert.False(seo.ContainsKey(key), seo.ToJsonString());
    }

    [Fact]
    public async Task Unset_geo_properties_are_left_out_so_the_API_keeps_them()
    {
        var (client, handler) = Capture();

        await Save(client, new PageItem { PageID = 7, Geo = new GeoProperties { Summary = "New summary", Ai = new GeoAi() } });

        var geo = SentBody(handler)["geo"]!.AsObject();
        Assert.Equal("New summary", (string?)geo["summary"]);
        Assert.False(geo.ContainsKey("schemaType"), geo.ToJsonString());
        Assert.False(geo.ContainsKey("structuredData"), geo.ToJsonString());
        Assert.Empty(geo["ai"]!.AsObject());
    }

    [Fact]
    public async Task Reading_a_page_then_saving_it_sends_geo_and_seo_back_unchanged()
    {
        var (client, handler) = Capture();

        var page = await client.Pages.GetPageAsync(TestClient.InstanceGuid, "en-us", 7, Ct);
        page.Title = "About (edited)";
        await Save(client, page);

        var body = SentBody(handler);
        var read = JsonNode.Parse(PageWithGeo)!;
        Assert.True(JsonNode.DeepEquals(read["geo"], body["geo"]), body["geo"]!.ToJsonString());
        Assert.True(JsonNode.DeepEquals(read["seo"], body["seo"]), body["seo"]!.ToJsonString());
        Assert.Equal(GeoSchemaTypes.AboutPage, page.Geo!.SchemaType);
        Assert.IsType<JsonArray>(page.Geo.StructuredData);
    }

    [Fact]
    public async Task Clear_methods_send_an_explicit_null()
    {
        var (client, handler) = Capture();
        var page = new PageItem
        {
            PageID = 7,
            Seo = new SeoProperties().ClearTitle().ClearOgImage(),
            Geo = new GeoProperties { SchemaType = GeoSchemaTypes.FAQPage }.ClearSummary().ClearStructuredData(),
        };
        page.Geo.Ai = new GeoAi { LlmsInclude = "exclude" }.ClearCrawl();

        await Save(client, page);

        var body = SentBody(handler);
        var seo = body["seo"]!.AsObject();
        Assert.True(seo.ContainsKey("title") && seo["title"] is null, seo.ToJsonString());
        Assert.True(seo.ContainsKey("ogImage") && seo["ogImage"] is null, seo.ToJsonString());
        Assert.False(seo.ContainsKey("canonicalUrl"));
        var geo = body["geo"]!.AsObject();
        Assert.Equal("FAQPage", (string?)geo["schemaType"]);
        Assert.True(geo.ContainsKey("summary") && geo["summary"] is null, geo.ToJsonString());
        Assert.True(geo.ContainsKey("structuredData") && geo["structuredData"] is null, geo.ToJsonString());
        var ai = geo["ai"]!.AsObject();
        Assert.Equal("exclude", (string?)ai["llmsInclude"]);
        Assert.True(ai.ContainsKey("crawl") && ai["crawl"] is null, ai.ToJsonString());
    }

    [Fact]
    public async Task A_value_set_after_a_clear_wins()
    {
        var (client, handler) = Capture();
        var geo = new GeoProperties().ClearSummary();
        geo.Summary = "Set again";

        await Save(client, new PageItem { PageID = 7, Geo = geo });

        var body = handler.Requests.Last().Body!;
        Assert.Equal(1, body.Split("\"summary\"").Length - 1);
        Assert.Equal("Set again", (string?)SentBody(handler)["geo"]!["summary"]);
    }
}
