using System.Globalization;
using System.Text.Json.Nodes;
using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk.IntegrationTests;

/// <summary>
/// Runs against a real instance. Skipped unless these environment variables are set:
/// <list type="bullet">
/// <item><c>AGILITY_MGMT_TOKEN</c>: a Personal Access Token for the instance.</item>
/// <item><c>AGILITY_INSTANCE_GUID</c>: the instance GUID. Use a test instance.</item>
/// <item><c>AGILITY_LOCALE</c>: optional, default <c>en-us</c>.</item>
/// <item><c>AGILITY_MGMT_BASE_URL</c>: optional, to point at a non-production API.</item>
/// <item><c>AGILITY_ALLOW_WRITES=true</c>: also run the tests that create, change and delete things.</item>
/// </list>
/// </summary>
public sealed class LiveApiTests : IDisposable
{
    private static readonly string? Token = Environment.GetEnvironmentVariable("AGILITY_MGMT_TOKEN");
    private static readonly string? InstanceGuid = Environment.GetEnvironmentVariable("AGILITY_INSTANCE_GUID");
    private static readonly string Locale = Environment.GetEnvironmentVariable("AGILITY_LOCALE") ?? "en-us";
    private static readonly bool AllowWrites = Environment.GetEnvironmentVariable("AGILITY_ALLOW_WRITES") == "true";

    private readonly AgilityManagementClient? _client;

    public LiveApiTests()
    {
        if (Token is null || InstanceGuid is null) return;
        var baseUrl = Environment.GetEnvironmentVariable("AGILITY_MGMT_BASE_URL");
        _client = new AgilityManagementClient(new AgilityManagementOptions
        {
            AccessToken = Token,
            BaseUrl = baseUrl is null ? null : new Uri(baseUrl),
            ApplicationName = "sdk-integration-tests",
        });
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AgilityInstanceClient Instance()
    {
        Assert.SkipWhen(_client is null, "Set AGILITY_MGMT_TOKEN and AGILITY_INSTANCE_GUID to run live tests.");
        return _client!.ForInstance(InstanceGuid!);
    }

    private AgilityInstanceClient WritableInstance()
    {
        var instance = Instance();
        Assert.SkipUnless(AllowWrites, "Set AGILITY_ALLOW_WRITES=true to run tests that change the instance.");
        return instance;
    }

    [Fact]
    public async Task Reads_the_instance_structure()
    {
        var instance = Instance();

        var locales = await instance.Locales.GetLocalesAsync(Ct);
        Assert.Contains(locales, l => string.Equals(l.LocaleCode, Locale, StringComparison.OrdinalIgnoreCase));

        var models = await instance.Models.GetContentModelsAsync(includeDefaults: true, cancellationToken: Ct);
        Assert.NotEmpty(models);

        var containers = await instance.Containers.GetContainerListAsync(cancellationToken: Ct);
        Assert.NotEmpty(containers);

        var sitemap = await instance.Pages.GetSitemapAsync(Locale, Ct);
        Assert.NotNull(sitemap);

        var templates = await instance.Pages.GetPageTemplatesAsync(Locale, includeModuleZones: true, cancellationToken: Ct);
        Assert.NotNull(templates);
    }

    [Fact]
    public async Task Reads_a_content_list_with_the_documented_POST_route()
    {
        var instance = Instance();
        var containers = await instance.Containers.GetContainerListAsync(cancellationToken: Ct);
        var list = containers.FirstOrDefault(c => c.IsListItem != true && !string.IsNullOrEmpty(c.ReferenceName) && c.ContentDefinitionType == 1);
        Assert.SkipWhen(list is null, "No content list on the instance.");

        var result = await instance.Content.GetContentListAsync(Locale, list!.ReferenceName!, take: 5, cancellationToken: Ct);

        Assert.True(result.Items is null || result.Items.Count <= 5);
    }

    [Fact]
    public async Task Reports_the_fetch_API_sync_status()
    {
        var status = await Instance().SyncStatus.GetFetchApiStatusAsync(SyncMode.Fetch, Ct);
        Assert.NotNull(status);
    }

    [Fact]
    public async Task Page_template_read_then_save_keeps_every_zones_default_components()
    {
        // The 1.x SDK cleared every zone's default components on this round trip (PROD-2376).
        var instance = WritableInstance();
        var component = (await instance.Models.GetComponentModelsAsync(includeDefault: true, Ct)).FirstOrDefault(m => m.Id > 0);
        Assert.SkipWhen(component is null, "No component model on the instance to use as a default component.");

        var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        PageModel? created = null;
        try
        {
            created = await instance.Pages.SavePageTemplateAsync(Locale, new PageModel
            {
                PageTemplateID = -1,
                PageTemplateName = $"SDK Test {suffix}",
                DigitalChannelTypeID = 1,
                ContentSectionDefinitions =
                [
                    new ContentSectionDefinition
                    {
                        PageItemTemplateID = -1,
                        PageItemTemplateName = "Main",
                        PageItemTemplateReferenceName = "Main",
                        ItemOrder = 0,
                        DefaultModules = [new ContentSectionDefaultModule { ContentDefinitionID = component!.Id, Title = component.DisplayName }],
                    },
                ],
            }, Ct);
            var id = created.PageTemplateID!.Value;

            static string Defaults(PageModel m) => string.Join("|", (m.ContentSectionDefinitions ?? [])
                .Select(z => $"{z.PageItemTemplateReferenceName}:{string.Join(",", (z.DefaultModules ?? []).Select(d => d.ContentDefinitionID))}"));

            var read = await instance.Pages.GetPageTemplateAsync(Locale, id, Ct);
            Assert.Equal($"Main:{component.Id}", Defaults(read));

            await instance.Pages.SavePageTemplateAsync(Locale, read, Ct);
            var afterRoundTrip = await instance.Pages.GetPageTemplateAsync(Locale, id, Ct);
            Assert.Equal($"Main:{component.Id}", Defaults(afterRoundTrip));

            // A rename with no zone list keeps the zones and their defaults.
            await instance.Pages.SavePageTemplateAsync(Locale, new PageModel
            {
                PageTemplateID = id,
                PageTemplateName = $"SDK Test {suffix} renamed",
                DigitalChannelTypeID = 1,
            }, Ct);
            Assert.Equal($"Main:{component.Id}", Defaults(await instance.Pages.GetPageTemplateAsync(Locale, id, Ct)));
        }
        finally
        {
            if (created?.PageTemplateID is int id) await instance.Pages.DeletePageTemplateAsync(Locale, id, CancellationToken.None);
        }
    }

    [Fact]
    public async Task Creates_saves_publishes_and_deletes_content_through_batches()
    {
        var instance = WritableInstance();
        var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        ContentModel? model = null;
        ContentContainer? container = null;
        int? contentId = null;
        try
        {
            model = await instance.Models.SaveModelAsync(new ContentModel
            {
                Id = 0,
                DisplayName = $"SDK Test {suffix}",
                ReferenceName = $"SdkTest{suffix}",
                Description = "Created by the .NET SDK integration tests; safe to delete.",
                Fields =
                [
                    new ContentModelField
                    {
                        Name = "Title", Label = "Title", Type = "Text", ItemOrder = 0, IsDataField = true, Editable = true,
                        Settings = new() { ["Required"] = "False", ["Length"] = "", ["DefaultValue"] = "", ["Unique"] = "False" },
                    },
                ],
            }, Ct);

            container = await instance.Containers.SaveContainerAsync(new ContentContainer
            {
                ContentViewID = 0,
                ContentDefinitionID = model.Id,
                ContentDefinitionTypeID = 1,
                ContentViewName = $"SDK Test {suffix}",
                ReferenceName = $"sdktest{suffix}",
                IsShared = false,
                IsDynamicPageList = true,
            }, cancellationToken: Ct);

            var saved = await instance.Content.SaveContentItemAsync(Locale, new ContentItem
            {
                ContentID = -1,
                Properties = new ContentItemProperties { ReferenceName = container.ReferenceName, DefinitionName = model.ReferenceName, ItemOrder = 0 },
                Fields = new JsonObject { ["title"] = "Hello from the .NET SDK" },
            }, cancellationToken: Ct);
            contentId = saved.ItemId;
            Assert.True(contentId > 0);

            var published = await instance.Content.PublishContentItemAsync(Locale, contentId!.Value, "SDK integration test", cancellationToken: Ct);
            Assert.True(published.IsProcessed);

            var item = await instance.Content.GetContentItemAsync(Locale, contentId.Value, Ct);
            Assert.Equal("Hello from the .NET SDK", item.Fields?["title"]?.GetValue<string>());
        }
        finally
        {
            if (contentId is int id) await instance.Content.DeleteContentItemAsync(Locale, id, cancellationToken: CancellationToken.None);
            if (container?.ContentViewID is int cid) await instance.Containers.DeleteContainerAsync(cid, CancellationToken.None);
            if (model?.Id is int mid) await instance.Models.DeleteModelAsync(mid, CancellationToken.None);
        }
    }

    public void Dispose() => _client?.Dispose();
}
