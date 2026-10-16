using System.Text.Json.Nodes;
using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk.Tests;

/// <summary>
/// An item in a list that backs a dynamic page carries its own <c>geo</c>, with the same three states on save as a
/// page's: left out keeps the stored value, an explicit <c>null</c> clears it, and a value replaces it (PROD-2726,
/// the MVP's item half). An unrelated save must never clear it.
/// </summary>
public class ContentItemGeoContractTests
{
    private const string ItemWithGeo = """
        {
          "contentID": 42,
          "properties": { "referenceName": "posts", "definitionName": "Post", "versionID": 3 },
          "fields": { "title": "A post" },
          "seo": { "metaDescription": "The post" },
          "geo": { "schemaType": "ItemPage", "summary": "The post in one sentence.",
                   "structuredData": { "@type": "BlogPosting", "headline": "A post" },
                   "ai": { "llmsInclude": "include", "crawl": "block" }, "futureKey": 1 }
        }
        """;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static (AgilityManagementClient Client, FakeHandler Handler) Capture()
    {
        var handler = new FakeHandler(r => r.Method == HttpMethod.Get ? FakeHandler.Json(ItemWithGeo) : FakeHandler.Json("9"));
        return (TestClient.Create(handler), handler);
    }

    private static JsonNode SentBody(FakeHandler handler) => JsonNode.Parse(handler.Requests.Last().Body!)!;

    private static Task<BatchResult> Save(AgilityManagementClient client, ContentItem item) =>
        client.Content.SaveContentItemAsync(TestClient.InstanceGuid, "en-us", item, waitForBatch: false, cancellationToken: Ct);

    [Fact]
    public async Task An_item_without_geo_is_saved_without_geo()
    {
        var (client, handler) = Capture();

        await Save(client, new ContentItem { ContentID = 42, Fields = new() { ["title"] = JsonValue.Create("A post")! } });

        Assert.False(SentBody(handler).AsObject().ContainsKey("geo"), handler.Requests.Last().Body);
    }

    [Fact]
    public async Task Reading_an_item_then_saving_it_sends_geo_back_unchanged()
    {
        var (client, handler) = Capture();

        var item = await client.Content.GetContentItemAsync(TestClient.InstanceGuid, "en-us", 42, Ct);
        await Save(client, item);

        var read = JsonNode.Parse(ItemWithGeo)!;
        var sent = SentBody(handler);
        Assert.True(JsonNode.DeepEquals(read["geo"], sent["geo"]), sent["geo"]!.ToJsonString());
        Assert.Equal(GeoSchemaTypes.ItemPage, item.Geo!.SchemaType);
        Assert.Equal("block", item.Geo.Ai!.Crawl);
    }

    [Fact]
    public async Task Clear_methods_on_item_geo_send_an_explicit_null_and_leave_unset_properties_out()
    {
        var (client, handler) = Capture();
        var item = new ContentItem { ContentID = 42, Geo = new GeoProperties { SchemaType = GeoSchemaTypes.FAQPage }.ClearSummary() };

        await Save(client, item);

        var geo = SentBody(handler)["geo"]!.AsObject();
        Assert.Equal("FAQPage", (string?)geo["schemaType"]);
        Assert.True(geo.ContainsKey("summary") && geo["summary"] is null, geo.ToJsonString());
        Assert.False(geo.ContainsKey("structuredData"), geo.ToJsonString());
        Assert.False(geo.ContainsKey("ai"), geo.ToJsonString());
    }

    [Fact]
    public async Task ClearGeo_sends_geo_null_to_clear_the_whole_block()
    {
        var (client, handler) = Capture();

        var item = await client.Content.GetContentItemAsync(TestClient.InstanceGuid, "en-us", 42, Ct);
        item.ClearGeo();
        await Save(client, item);

        var sent = SentBody(handler).AsObject();
        Assert.Null(item.Geo);
        Assert.True(sent.ContainsKey("geo") && sent["geo"] is null, sent.ToJsonString());
        Assert.Equal(1, handler.Requests.Last().Body!.Split("\"geo\"").Length - 1);
        Assert.NotNull(sent["seo"]);
    }

    [Fact]
    public async Task Geo_set_after_ClearGeo_cancels_the_clear()
    {
        var (client, handler) = Capture();
        var item = new ContentItem { ContentID = 42 }.ClearGeo();
        item.Geo = new GeoProperties { Summary = "Set again" };

        await Save(client, item);

        Assert.Equal(1, handler.Requests.Last().Body!.Split("\"geo\"").Length - 1);
        Assert.Equal("Set again", (string?)SentBody(handler)["geo"]!["summary"]);
    }

    [Fact]
    public async Task SaveContentItems_sends_each_items_geo_state()
    {
        var (client, handler) = Capture();
        var items = new List<ContentItem>
        {
            new() { ContentID = 1, Geo = new GeoProperties { Summary = "One" } },
            new ContentItem { ContentID = 2 }.ClearGeo(),
            new() { ContentID = 3 },
        };

        await client.Content.SaveContentItemsAsync(TestClient.InstanceGuid, "en-us", items, waitForBatch: false, cancellationToken: Ct);

        var sent = SentBody(handler).AsArray();
        Assert.Equal("One", (string?)sent[0]!["geo"]!["summary"]);
        Assert.True(sent[1]!.AsObject().ContainsKey("geo") && sent[1]!["geo"] is null, sent.ToJsonString());
        Assert.False(sent[2]!.AsObject().ContainsKey("geo"), sent.ToJsonString());
    }

    [Fact]
    public async Task A_response_with_geo_null_reads_and_saves_without_geo()
    {
        var handler = new FakeHandler(r => r.Method == HttpMethod.Get
            ? FakeHandler.Json("""{ "contentID": 42, "geo": null, "unknownItemKey": 1 }""")
            : FakeHandler.Json("9"));
        var client = TestClient.Create(handler);

        var item = await client.Content.GetContentItemAsync(TestClient.InstanceGuid, "en-us", 42, Ct);
        Assert.Null(item.Geo);
        await Save(client, item);

        var sent = SentBody(handler).AsObject();
        Assert.False(sent.ContainsKey("geo"), sent.ToJsonString());
        Assert.False(sent.ContainsKey("unknownItemKey"), sent.ToJsonString());
    }
}
