using System.Text.Json;
using System.Text.Json.Nodes;
using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk.Tests;

/// <summary>
/// The client-contract rules the spec can't express (system-overview components/management-api.md §6.1),
/// checked on the request bodies the SDK actually sends.
/// </summary>
public class ContractTests
{
    private const string TemplateWithOneDefault = """
        {
          "pageTemplateID": 12,
          "pageTemplateName": "Main Template",
          "contentSectionDefinitions": [
            {
              "pageItemTemplateID": 34,
              "pageItemTemplateName": "Main Zone",
              "pageItemTemplateReferenceName": "MainZone",
              "defaultModules": [
                { "title": "Rich Text", "pageItemTemplateDefaultModuleID": null, "contentDefinitionID": 78, "pageItemTemplateID": 34, "autoCreate": true }
              ]
            }
          ]
        }
        """;

    private static (AgilityInstanceClient Instance, FakeHandler Handler) Capture(string responseJson = TemplateWithOneDefault)
    {
        var handler = new FakeHandler(_ => FakeHandler.Json(responseJson));
        return (TestClient.Instance(handler), handler);
    }

    private static JsonObject SentBody(FakeHandler handler) =>
        JsonNode.Parse(handler.Requests.Last().Body!)!.AsObject();

    [Fact]
    public async Task A_new_zone_leaves_default_and_shared_modules_out_so_the_API_keeps_them()
    {
        var (instance, handler) = Capture();
        var template = new PageModel
        {
            PageTemplateName = "Main Template",
            ContentSectionDefinitions = [new ContentSectionDefinition { PageItemTemplateName = "Main Zone" }],
        };

        await instance.Pages.SavePageTemplateAsync("en-us", template, TestContext.Current.CancellationToken);

        var zone = SentBody(handler)["contentSectionDefinitions"]![0]!.AsObject();
        Assert.False(zone.ContainsKey("defaultModules"), zone.ToJsonString());
        Assert.False(zone.ContainsKey("sharedModules"), zone.ToJsonString());
    }

    [Fact]
    public async Task Reading_a_template_then_saving_it_keeps_each_zones_default_modules()
    {
        var (instance, handler) = Capture();
        var ct = TestContext.Current.CancellationToken;

        var template = await instance.Pages.GetPageTemplateAsync("en-us", 12, ct);
        Assert.Single(template.ContentSectionDefinitions![0].DefaultModules!);

        await instance.Pages.SavePageTemplateAsync("en-us", template, ct);

        var sent = SentBody(handler)["contentSectionDefinitions"]![0]!["defaultModules"]!.AsArray();
        var module = Assert.Single(sent)!;
        Assert.Equal(78, module["contentDefinitionID"]!.GetValue<int>());
        Assert.True(module["autoCreate"]!.GetValue<bool>());
    }

    [Fact]
    public async Task An_explicitly_empty_default_modules_list_is_sent_as_an_empty_array_to_clear_them()
    {
        var (instance, handler) = Capture();
        var template = new PageModel { ContentSectionDefinitions = [new ContentSectionDefinition { DefaultModules = [] }] };

        await instance.Pages.SavePageTemplateAsync("en-us", template, TestContext.Current.CancellationToken);

        var modules = SentBody(handler)["contentSectionDefinitions"]![0]!["defaultModules"];
        Assert.Equal(JsonValueKind.Array, modules!.GetValueKind());
        Assert.Empty(modules.AsArray());
    }

    [Fact]
    public async Task A_template_with_no_zone_list_leaves_the_zones_out_so_the_API_keeps_them()
    {
        var (instance, handler) = Capture();

        await instance.Pages.SavePageTemplateAsync("en-us", new PageModel { PageTemplateID = 12, PageTemplateName = "Renamed" },
            TestContext.Current.CancellationToken);

        Assert.False(SentBody(handler).ContainsKey("contentSectionDefinitions"));
    }

    [Fact]
    public async Task Content_fields_round_trip_unchanged_from_read_to_save()
    {
        const string item = """
            {"contentID":7,"properties":{"state":2,"definitionName":"Post","referenceName":"posts","itemOrder":1},
             "fields":{"title":"Hello","count":3,"published":true,"tags":["a","b"],"image":{"url":"https://x/y.jpg","label":null},"empty":null}}
            """;
        var handler = new FakeHandler(r => r.Method == HttpMethod.Get ? FakeHandler.Json(item) : FakeHandler.Json("9"));
        var instance = TestClient.Instance(handler);
        var ct = TestContext.Current.CancellationToken;

        var read = await instance.Content.GetContentItemAsync("en-us", 7, ct);
        await instance.Content.SaveContentItemAsync("en-us", read, waitForBatch: false, ct);

        var sentFields = SentBody(handler)["fields"]!;
        var readFields = JsonNode.Parse(item)!["fields"]!;
        Assert.True(JsonNode.DeepEquals(readFields, sentFields), sentFields.ToJsonString());
    }

    [Fact]
    public async Task Unset_collections_on_other_saves_are_left_out_too()
    {
        var handler = new FakeHandler(_ => FakeHandler.Json("{}"));
        var instance = TestClient.Instance(handler);

        await instance.Models.SaveModelAsync(new ContentModel { DisplayName = "Post", ReferenceName = "post" }, TestContext.Current.CancellationToken);

        Assert.False(SentBody(handler).ContainsKey("fields"));
    }

    [Fact]
    public async Task Null_properties_are_left_out_because_the_API_rejects_explicit_nulls()
    {
        // The live API answers {"genericSearch":null} with 400 "The GenericSearch field is required."
        var handler = new FakeHandler(_ => FakeHandler.Json("""{"totalCount":0,"items":[]}"""));
        var instance = TestClient.Instance(handler);

        await instance.Content.GetContentListAsync("en-us", "posts", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("{}", handler.Requests.Single().Body);

        await instance.Content.GetContentListAsync("en-us", "posts", new ContentListFilterModel { GenericSearch = "x" },
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("""{"genericSearch":"x"}""", handler.Requests.Last().Body);
    }

    [Fact]
    public async Task Workflow_operations_use_GET_as_the_API_requires()
    {
        var handler = new FakeHandler(r => r.Uri.AbsolutePath.EndsWith("/batch/41", StringComparison.Ordinal)
            ? FakeHandler.Json(TestClient.ProcessedBatch(41)) : FakeHandler.Json("41"));
        var instance = TestClient.Instance(handler);

        await instance.Content.PublishContentItemAsync("en-us", 7, cancellationToken: TestContext.Current.CancellationToken);

        var publish = handler.Requests[0];
        Assert.Equal(HttpMethod.Get, publish.Method);
        Assert.EndsWith("/en-us/item/7/publish", publish.Uri.AbsolutePath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Batch_status_is_read_from_the_instance_level_route_not_a_locale_route()
    {
        var handler = new FakeHandler(r => r.Uri.AbsolutePath.Contains("/batch/", StringComparison.Ordinal)
            ? FakeHandler.Json(TestClient.ProcessedBatch(41)) : FakeHandler.Json("41"));
        var instance = TestClient.Instance(handler);

        await instance.Pages.PublishPageAsync("en-us", 3, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal($"/api/v1/instance/{TestClient.InstanceGuid}/batch/41", handler.Requests[1].Uri.AbsolutePath);
    }
}
