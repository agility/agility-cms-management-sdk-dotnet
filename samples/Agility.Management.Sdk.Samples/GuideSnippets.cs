// The examples from docs/*.md, so the build catches any that stop compiling. They aren't run.
// Keep them in step with the guides: if you change one, change the other.

using System.Net;
using System.Text.Json.Nodes;
using Agility.Management.Sdk;
using Agility.Management.Sdk.Clients;
using Agility.Management.Sdk.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Agility.Management.Sdk.Samples;

internal static class ConceptsGuide
{
    public static async Task EditLiveItem(AgilityManagementClient client, string guid, int id)
    {
        var item = await client.Content.GetContentItemAsync(guid, "en-us", id);
        item.Fields!["title"] = "New title";
        await client.Content.SaveContentItemAsync(guid, "en-us", item);
        await client.Content.PublishContentItemAsync(guid, "en-us", id);
    }

    public static async Task FireAndForget(AgilityManagementClient client, string guid, int id)
    {
        var queued = await client.Content.PublishContentItemAsync(guid, "en-us", id, waitForBatch: false);
        var batch = await client.Batches.WaitForBatchAsync(guid, queued.BatchId);
        await client.SyncStatus.WaitForFetchApiSyncAsync(guid, SyncMode.Fetch);
    }

    public static async Task HandleErrors(AgilityManagementClient client, string guid, ContentItem item)
    {
        try
        {
            await client.Content.SaveContentItemAsync(guid, "en-us", item);
        }
        catch (AgilityBatchException ex)
        {
            foreach (var failed in ex.Batch?.Items?.Where(i => i.ErrorMessage is not null) ?? [])
                Console.WriteLine($"{failed.ItemID}: {failed.ErrorMessage}");
        }
        catch (AgilityManagementException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine(ex.ApiMessage);
        }
    }
}

internal static class AuthenticationGuide
{
    public static async Task OAuth(string csrfToken, string code, string storedRefreshToken)
    {
        using var signInClient = new AgilityManagementClient(new AgilityManagementOptions());
        var signIn = signInClient.OAuth.GetAuthorizeUri(new Uri("https://myapp.example.com/agility/callback"), state: csrfToken);
        TokenResponseData tokens = await signInClient.OAuth.ExchangeCodeAsync(code);

        using var client = new AgilityManagementClient(new AgilityManagementOptions
        {
            RefreshToken = storedRefreshToken,
            RefreshTokenChanged = newToken => Console.WriteLine("store " + newToken.Length),
        });
    }

    public sealed class VaultTokenProvider(Func<CancellationToken, Task<string>> vault) : IAccessTokenProvider
    {
        public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken) => await vault(cancellationToken);
    }

    public static void DependencyInjection(IServiceCollection services, string? token)
    {
        services.AddAgilityManagement(o =>
        {
            o.AccessToken = token;
            o.ApplicationName = "my-sync-service/1.0";
        });
    }
}

internal static class ContentGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid)
    {
        ContentItem item = await client.Content.GetContentItemAsync(guid, "en-us", 42);
        string? title = item.Fields?["title"]?.GetValue<string>();
        List<ContentItem> items = await client.Content.GetContentItemsByIdAsync(guid, "en-us", [42, 43, 44]);

        ContentList page = await client.Content.GetContentListAsync(guid, "en-us", "blogposts",
            new ContentListOptions { Take = 50, Skip = 0, SortField = "title", SortDirection = "asc" });
        foreach (JsonNode? row in page.Items ?? [])
            Console.WriteLine(row?["contentID"]);

        var filter = new ContentListFilterModel
        {
            GenericSearch = "launch",
            StateIds = [(int)ItemState.Published],
            DateRange = new DateRangeFilter { StartDate = DateTime.UtcNow.AddDays(-30) },
            FieldFilters = [new FieldFilter { Field = "category", Value = new FieldFilterValue { StringValue = "news" } }],
        };
        var recent = await client.Content.GetContentListAsync(guid, "en-us", "blogposts",
            new ContentListOptions { Filter = filter, Take = 20 });

        var created = await client.Content.SaveContentItemAsync(guid, "en-us", new ContentItem
        {
            ContentID = -1,
            Properties = new ContentItemProperties { ReferenceName = "blogposts", DefinitionName = "BlogPost" },
            Fields = new JsonObject { ["title"] = "Hello", ["body"] = "<p>First post</p>" },
        });
        int newId = created.ItemId!.Value;

        BatchResult result = await client.Content.SaveContentItemsAsync(guid, "en-us", [item, item]);
        IReadOnlyList<int> ids = result.ItemIds;

        await client.Content.PublishContentItemAsync(guid, "en-us", 42, comments: "Launch");
        await client.Content.UnpublishContentItemAsync(guid, "en-us", 42);
        await client.Content.RequestApprovalContentItemAsync(guid, "en-us", 42);
        await client.Content.ApproveContentItemAsync(guid, "en-us", 42);
        await client.Content.DeclineContentItemAsync(guid, "en-us", 42, comments: "Needs a new image");
        await client.Content.DeleteContentItemAsync(guid, "en-us", 42);
        await client.Content.BatchWorkflowContentItemsAsync(guid, "en-us", [42, 43, 44], WorkflowOperationType.Publish);

        CascadeItem tree = await client.Content.GetCascadeItemsAsync(guid, "en-us", 42);
        BatchCreateResult cascade = await client.Content.PublishContentItemCascadeAsync(guid, "en-us", 42);
        foreach (var batchId in cascade.BatchIDs ?? [])
            await client.Batches.WaitForBatchAsync(guid, batchId);

        ContentItemHistoryResponse history = await client.Content.GetContentItemHistoryAsync(guid, "en-us", 42, take: 20);
        ItemCommentsResponse comments = await client.Content.GetContentItemCommentsAsync(guid, "en-us", 42);

        BatchCreateResult batch = await client.Batches.CreateBatchAsync(guid, new CreateBatchWithItemsRequest
        {
            BatchName = "Spring launch",
            Operation = WorkflowOperationType.Publish,
            Items =
            [
                new AddBatchItemRequest { ItemType = BatchItemType.ContentItem, ItemID = 42, LanguageCode = "en-us" },
                new AddBatchItemRequest { ItemType = BatchItemType.Page, ItemID = 7, LanguageCode = "en-us" },
            ],
        }, processNow: true);
    }
}

internal static class PagesGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid, int templateId)
    {
        List<Sitemap> sitemap = await client.Pages.GetSitemapAsync(guid, "en-us");
        PageItem page = await client.Pages.GetPageAsync(guid, "en-us", 7);
        foreach (var (zone, modules) in page.Zones ?? [])
            Console.WriteLine($"{zone}: {modules.Count} components");

        page.Title = "About us";
        await client.Pages.SavePageAsync(guid, "en-us", page);
        var created = await client.Pages.SavePageAsync(guid, "en-us", page, new SavePageOptions { ParentPageId = 7 });
        await client.Pages.PublishPageAsync(guid, "en-us", 7);

        List<PageModel> templates = await client.Pages.GetPageTemplatesAsync(guid, "en-us", includeModuleZones: true);
        PageModel byName = await client.Pages.GetPageTemplateByNameAsync(guid, "en-us", "Main Template");

        var template = await client.Pages.GetPageTemplateAsync(guid, "en-us", templateId);
        template.PageTemplateName = "Landing Page";
        template.ContentSectionDefinitions = null;
        await client.Pages.SavePageTemplateAsync(guid, "en-us", template);

        template = await client.Pages.GetPageTemplateAsync(guid, "en-us", templateId);
        template.ContentSectionDefinitions!.Add(new ContentSectionDefinition
        {
            PageItemTemplateID = -1,
            PageItemTemplateName = "Sidebar",
            PageItemTemplateReferenceName = "Sidebar",
        });
        await client.Pages.SavePageTemplateAsync(guid, "en-us", template);

        template.ContentSectionDefinitions!.Single(z => z.PageItemTemplateReferenceName == "Sidebar").DefaultModules = [];
        await client.Pages.SavePageTemplateAsync(guid, "en-us", template);
    }
}

internal static class ModelsGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid)
    {
        List<ContentModel> contentModels = await client.Models.GetContentModelsAsync(guid, includeDefaults: false);
        List<ContentModel> componentModels = await client.Models.GetComponentModelsAsync(guid);
        ContentModel model = await client.Models.GetModelByReferenceNameAsync(guid, "BlogPost");
        ContentModel byId = await client.Models.GetModelAsync(guid, model.Id!.Value);

        var saved = await client.Models.SaveModelAsync(guid, new ContentModel
        {
            Id = 0,
            DisplayName = "Blog Post",
            ReferenceName = "BlogPost",
            Fields =
            [
                new ContentModelField
                {
                    Name = "Title", Label = "Title", Type = "Text", IsDataField = true, Editable = true,
                    Settings = new() { ["Required"] = "True" },
                },
            ],
        });

        List<ContentContainer> all = await client.Containers.GetContainerListAsync(guid);
        ContentContainer posts = await client.Containers.GetContainerByReferenceNameAsync(guid, "blogposts");
        List<ContentContainer> forModel = await client.Containers.GetContainersByModelAsync(guid, model.Id!.Value);
        ContentContainerPagedResult page = await client.Containers.GetContainerListPagedAsync(guid,
            new ContainerListOptions { PageSize = 100, RecordOffset = 0 });

        var container = await client.Containers.SaveContainerAsync(guid, new ContentContainer
        {
            ContentViewID = 0,
            ContentDefinitionID = model.Id,
            ContentDefinitionTypeID = 1,
            ContentViewName = "Blog Posts",
            ReferenceName = "blogposts",
            IsShared = false,
            IsDynamicPageList = true,
        });
    }
}

internal static class AssetsGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid, int mediaId, int galleryId)
    {
        await using var file = File.OpenRead("hero.jpg");
        List<AssetMedia> uploaded = await client.Assets.UploadAsync(guid, "images/blog", [new AssetUpload("hero.jpg", file, "image/jpeg")]);
        Console.WriteLine(uploaded[0].EdgeUrl);

        AssetMediaList page = await client.Assets.GetMediaListAsync(guid, pageSize: 100, recordOffset: 0);
        AssetMedia asset = await client.Assets.GetAssetAsync(guid, mediaId);
        AssetMedia byUrl = await client.Assets.GetAssetByUrlAsync(guid, "https://cdn.aglty.io/abc/images/blog/hero.jpg");
        AssetContainer container = await client.Assets.GetDefaultContainerAsync(guid);

        await client.Assets.CreateFolderAsync(guid, "images/blog/2026");
        await client.Assets.RenameFolderAsync(guid, "images/blog/2026", "images/blog/archive-2026");
        await client.Assets.DeleteFolderAsync(guid, "images/blog/archive-2026");
        await client.Assets.MoveAssetAsync(guid, mediaId, "images/blog/archive-2026");
        await client.Assets.DeleteAssetAsync(guid, mediaId);

        AssetGalleries galleries = await client.Assets.GetGalleriesAsync(guid, search: "team");
        AssetMediaGrouping gallery = await client.Assets.GetGalleryAsync(guid, galleryId);
        AssetMediaGrouping? byName = await client.Assets.GetGalleryByNameAsync(guid, "Team photos");
        var created = await client.Assets.SaveGalleryAsync(guid, new AssetMediaGrouping { MediaGroupingID = -1, Name = "Team photos", GroupingTypeID = 1 });
        await client.Assets.DeleteGalleryAsync(guid, created.MediaGroupingID!.Value);
    }
}

internal static class LocalizationGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid, int localeId)
    {
        List<Locale> enabled = await client.Locales.GetLocalesAsync(guid);
        LocalesResponse all = await client.Locales.GetAllLocalesAsync(guid);
        Locale fr = await client.Locales.GetLocaleAsync(guid, localeId);

        var added = await client.Locales.SaveLocaleAsync(guid, new Locale { LocaleName = "French (Canada)", LocaleCode = "fr-ca" })
            ?? throw new InvalidOperationException("The API didn't return the saved locale.");
        await client.Locales.EnableLocaleAsync(guid, added.LocaleID!.Value);
        await client.Locales.DisableLocaleAsync(guid, added.LocaleID!.Value);
        await client.Locales.SetSortOrderAsync(guid, [1, added.LocaleID!.Value, 3]);

        var item = await client.Content.GetContentItemAsync(guid, "en-us", 42);
        await client.Localization.TranslateContentItemsAsync(guid, new TranslateContentRequest
        {
            LanguageCodeSource = "en-us",
            LanguageCodeTargets = ["fr-ca", "es-us"],
            ContentVersionIds = [item.Properties!.VersionID!.Value],
        });
    }
}

internal static class WebhooksGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid)
    {
        var webhook = await client.Webhooks.SaveWebhookAsync(guid, new Webhook
        {
            Name = "Rebuild site",
            Url = "https://build.example.com/hooks/agility",
            Enabled = true,
            ContentPublishEvents = true,
            ContentSaveEvents = false,
            ContentWorkflowEvents = false,
            SecureDeliveryEnabled = true,
        });
        if (webhook.SigningSecretJustCreated) Console.WriteLine(webhook.SigningSecret!.Length);

        var rotated = await client.Webhooks.RotateSigningSecretAsync(guid, webhook.RowKey!);

        string? token = null;
        do
        {
            var page = await client.Webhooks.GetWebhooksAsync(guid, take: 50, continuationToken: token);
            foreach (var hook in page.Items ?? []) Console.WriteLine(hook.Name);
            token = page.Token;
        }
        while (token is not null);

        var history = await client.Webhooks.GetWebhookHistoryAsync(guid, webhook.RowKey!, fromDate: DateTime.UtcNow.AddDays(-1));
        await client.Webhooks.DeleteWebhookAsync(guid, webhook.RowKey!);
    }
}

internal static class UrlRedirectionsGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid)
    {
        UrlRedirectionSaveResult saved = await client.UrlRedirections.SaveUrlRedirectionsAsync(guid,
        [
            new UrlRedirection { UrlRedirectionID = 0, OriginUrl = "/old-page", DestinationUrl = "/new-page", HttpCode = 301 },
            new UrlRedirection { UrlRedirectionID = 0, OriginUrl = "/promo", DestinationUrl = "https://example.com/sale", HttpCode = 302 },
        ]);
        UrlRedirectionDeleteResult deleted = await client.UrlRedirections.DeleteUrlRedirectionsAsync(guid, [12, 13]);

        await using (var export = await client.UrlRedirections.ExportUrlRedirectionsAsync(guid))
        await using (var file = File.Create("redirections.xlsx"))
            await export.CopyToAsync(file);

        await using var edited = File.OpenRead("redirections.xlsx");
        UrlRedirectionSaveResult imported = await client.UrlRedirections.ImportUrlRedirectionsAsync(guid, edited);
    }
}

internal static class UsersGuide
{
    public static async Task Examples(AgilityManagementClient client, string guid, int editorRoleId)
    {
        ServerUser me = await client.ServerUsers.GetCurrentUserAsync();
        foreach (var site in me.WebsiteAccess ?? [])
            Console.WriteLine($"{site.WebsiteName}: {site.Guid}");

        List<WebsiteUser> users = await client.InstanceUsers.GetUsersAsync(guid);
        InstanceUser user = await client.InstanceUsers.SaveUserAsync(guid, "editor@example.com",
            [new InstanceRole { RoleID = editorRoleId }], firstName: "Sam", lastName: "Lee");
        await client.InstanceUsers.DeleteUserAsync(guid, user.UserID);

        var created = await client.PersonalAccessTokens.CreateTokenAsync(new PersonalAccessTokenRequest
        {
            Name = "nightly-sync",
            ExpiryDate = DateTime.UtcNow.AddYears(1),
        });
        PersonalAccessTokenListResponse mine = await client.PersonalAccessTokens.GetTokensAsync();
        await client.PersonalAccessTokens.UpdateTokenAsync(created.TokenID, new PersonalAccessTokenUpdateRequest { Enabled = false });
        await client.PersonalAccessTokens.RevokeTokenAsync(created.TokenID);

        string fetchKey = await client.OAuth.GetFetchApiKeyAsync(guid);
        string previewKey = await client.OAuth.GetPreviewApiKeyAsync(guid);
        AllTypesResponse types = await client.Types.GetAllTypesAsync();
    }
}
