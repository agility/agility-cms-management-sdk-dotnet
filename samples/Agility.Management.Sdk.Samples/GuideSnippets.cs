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
    public static async Task EditLiveItem(AgilityInstanceClient instance, int id)
    {
        var item = await instance.Content.GetContentItemAsync("en-us", id);
        item.Fields!["title"] = "New title";
        await instance.Content.SaveContentItemAsync("en-us", item);
        await instance.Content.PublishContentItemAsync("en-us", id);
    }

    public static async Task FireAndForget(AgilityInstanceClient instance, int id)
    {
        var queued = await instance.Content.PublishContentItemAsync("en-us", id, waitForBatch: false);
        var batch = await instance.Batches.WaitForBatchAsync(queued.BatchId);
        await instance.SyncStatus.WaitForFetchApiSyncAsync(SyncMode.Fetch);
    }

    public static async Task HandleErrors(AgilityInstanceClient instance, ContentItem item)
    {
        try
        {
            await instance.Content.SaveContentItemAsync("en-us", item);
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
    public static async Task Examples(AgilityInstanceClient instance)
    {
        ContentItem item = await instance.Content.GetContentItemAsync("en-us", 42);
        string? title = item.Fields?["title"]?.GetValue<string>();
        List<ContentItem> items = await instance.Content.GetContentItemsByIdAsync("en-us", [42, 43, 44]);

        ContentList page = await instance.Content.GetContentListAsync("en-us", "blogposts", take: 50, skip: 0,
            sortField: "title", sortDirection: "asc");
        foreach (JsonNode? row in page.Items ?? [])
            Console.WriteLine(row?["contentID"]);

        var filter = new ContentListFilterModel
        {
            GenericSearch = "launch",
            StateIds = [(int)ItemState.Published],
            DateRange = new DateRangeFilter { StartDate = DateTime.UtcNow.AddDays(-30) },
            FieldFilters = [new FieldFilter { Field = "category", Value = new FieldFilterValue { StringValue = "news" } }],
        };
        var recent = await instance.Content.GetContentListAsync("en-us", "blogposts", filter, take: 20);

        var created = await instance.Content.SaveContentItemAsync("en-us", new ContentItem
        {
            ContentID = -1,
            Properties = new ContentItemProperties { ReferenceName = "blogposts", DefinitionName = "BlogPost" },
            Fields = new JsonObject { ["title"] = "Hello", ["body"] = "<p>First post</p>" },
        });
        int newId = created.ItemId!.Value;

        BatchResult result = await instance.Content.SaveContentItemsAsync("en-us", [item, item]);
        IReadOnlyList<int> ids = result.ItemIds;

        await instance.Content.PublishContentItemAsync("en-us", 42, comments: "Launch");
        await instance.Content.UnpublishContentItemAsync("en-us", 42);
        await instance.Content.RequestApprovalContentItemAsync("en-us", 42);
        await instance.Content.ApproveContentItemAsync("en-us", 42);
        await instance.Content.DeclineContentItemAsync("en-us", 42, comments: "Needs a new image");
        await instance.Content.DeleteContentItemAsync("en-us", 42);
        await instance.Content.BatchWorkflowContentItemsAsync("en-us", [42, 43, 44], WorkflowOperationType.Publish);

        CascadeItem tree = await instance.Content.GetCascadeItemsAsync("en-us", 42);
        BatchCreateResult cascade = await instance.Content.PublishContentItemCascadeAsync("en-us", 42);
        foreach (var batchId in cascade.BatchIDs ?? [])
            await instance.Batches.WaitForBatchAsync(batchId);

        ContentItemHistoryResponse history = await instance.Content.GetContentItemHistoryAsync("en-us", 42, take: 20);
        ItemCommentsResponse comments = await instance.Content.GetContentItemCommentsAsync("en-us", 42);

        BatchCreateResult batch = await instance.Batches.CreateBatchAsync(new CreateBatchWithItemsRequest
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
    public static async Task Examples(AgilityInstanceClient instance, int templateId)
    {
        List<Sitemap> sitemap = await instance.Pages.GetSitemapAsync("en-us");
        PageItem page = await instance.Pages.GetPageAsync("en-us", 7);
        foreach (var (zone, modules) in page.Zones ?? [])
            Console.WriteLine($"{zone}: {modules.Count} components");

        page.Title = "About us";
        await instance.Pages.SavePageAsync("en-us", page);
        await instance.Pages.PublishPageAsync("en-us", 7);

        List<PageModel> templates = await instance.Pages.GetPageTemplatesAsync("en-us", includeModuleZones: true);
        PageModel byName = await instance.Pages.GetPageTemplateByNameAsync("en-us", "Main Template");

        var template = await instance.Pages.GetPageTemplateAsync("en-us", templateId);
        template.PageTemplateName = "Landing Page";
        template.ContentSectionDefinitions = null;
        await instance.Pages.SavePageTemplateAsync("en-us", template);

        template = await instance.Pages.GetPageTemplateAsync("en-us", templateId);
        template.ContentSectionDefinitions!.Add(new ContentSectionDefinition
        {
            PageItemTemplateID = -1,
            PageItemTemplateName = "Sidebar",
            PageItemTemplateReferenceName = "Sidebar",
        });
        await instance.Pages.SavePageTemplateAsync("en-us", template);

        template.ContentSectionDefinitions!.Single(z => z.PageItemTemplateReferenceName == "Sidebar").DefaultModules = [];
        await instance.Pages.SavePageTemplateAsync("en-us", template);
    }
}

internal static class ModelsGuide
{
    public static async Task Examples(AgilityInstanceClient instance)
    {
        List<ContentModel> contentModels = await instance.Models.GetContentModelsAsync(includeDefaults: false);
        List<ContentModel> componentModels = await instance.Models.GetComponentModelsAsync();
        ContentModel model = await instance.Models.GetModelByReferenceNameAsync("BlogPost");
        ContentModel byId = await instance.Models.GetModelAsync(model.Id!.Value);

        var saved = await instance.Models.SaveModelAsync(new ContentModel
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

        List<ContentContainer> all = await instance.Containers.GetContainerListAsync();
        ContentContainer posts = await instance.Containers.GetContainerByReferenceNameAsync("blogposts");
        List<ContentContainer> forModel = await instance.Containers.GetContainersByModelAsync(model.Id!.Value);
        ContentContainerPagedResult page = await instance.Containers.GetContainerListPagedAsync(pageSize: 100, recordOffset: 0);

        var container = await instance.Containers.SaveContainerAsync(new ContentContainer
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
    public static async Task Examples(AgilityInstanceClient instance, int mediaId, int galleryId)
    {
        await using var file = File.OpenRead("hero.jpg");
        List<AssetMedia> uploaded = await instance.Assets.UploadAsync("images/blog", [new AssetUpload("hero.jpg", file, "image/jpeg")]);
        Console.WriteLine(uploaded[0].EdgeUrl);

        AssetMediaList page = await instance.Assets.GetMediaListAsync(pageSize: 100, recordOffset: 0);
        AssetMedia asset = await instance.Assets.GetAssetAsync(mediaId);
        AssetMedia byUrl = await instance.Assets.GetAssetByUrlAsync("https://cdn.aglty.io/abc/images/blog/hero.jpg");
        AssetContainer container = await instance.Assets.GetDefaultContainerAsync();

        await instance.Assets.CreateFolderAsync("images/blog/2026");
        await instance.Assets.RenameFolderAsync("images/blog/2026", "images/blog/archive-2026");
        await instance.Assets.DeleteFolderAsync("images/blog/archive-2026");
        await instance.Assets.MoveAssetAsync(mediaId, "images/blog/archive-2026");
        await instance.Assets.DeleteAssetAsync(mediaId);

        AssetGalleries galleries = await instance.Assets.GetGalleriesAsync(search: "team");
        AssetMediaGrouping gallery = await instance.Assets.GetGalleryAsync(galleryId);
        AssetMediaGrouping? byName = await instance.Assets.GetGalleryByNameAsync("Team photos");
        var created = await instance.Assets.SaveGalleryAsync(new AssetMediaGrouping { MediaGroupingID = -1, Name = "Team photos", GroupingTypeID = 1 });
        await instance.Assets.DeleteGalleryAsync(created.MediaGroupingID!.Value);
    }
}

internal static class LocalizationGuide
{
    public static async Task Examples(AgilityInstanceClient instance, int localeId)
    {
        List<Locale> enabled = await instance.Locales.GetLocalesAsync();
        LocalesResponse all = await instance.Locales.GetAllLocalesAsync();
        Locale fr = await instance.Locales.GetLocaleAsync(localeId);

        var added = await instance.Locales.SaveLocaleAsync(new Locale { LocaleName = "French (Canada)", LocaleCode = "fr-ca" })
            ?? throw new InvalidOperationException("The API didn't return the saved locale.");
        await instance.Locales.EnableLocaleAsync(added.LocaleID!.Value);
        await instance.Locales.DisableLocaleAsync(added.LocaleID!.Value);
        await instance.Locales.SetSortOrderAsync([1, added.LocaleID!.Value, 3]);

        var item = await instance.Content.GetContentItemAsync("en-us", 42);
        await instance.Localization.TranslateContentItemsAsync(new TranslateContentRequest
        {
            LanguageCodeSource = "en-us",
            LanguageCodeTargets = ["fr-ca", "es-us"],
            ContentVersionIds = [item.Properties!.VersionID!.Value],
        });
    }
}

internal static class WebhooksGuide
{
    public static async Task Examples(AgilityInstanceClient instance)
    {
        var webhook = await instance.Webhooks.SaveWebhookAsync(new Webhook
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

        var rotated = await instance.Webhooks.RotateSigningSecretAsync(webhook.RowKey!);

        string? token = null;
        do
        {
            var page = await instance.Webhooks.GetWebhooksAsync(take: 50, continuationToken: token);
            foreach (var hook in page.Items ?? []) Console.WriteLine(hook.Name);
            token = page.Token;
        }
        while (token is not null);

        var history = await instance.Webhooks.GetWebhookHistoryAsync(webhook.RowKey!, fromDate: DateTime.UtcNow.AddDays(-1));
        await instance.Webhooks.DeleteWebhookAsync(webhook.RowKey!);
    }
}

internal static class UrlRedirectionsGuide
{
    public static async Task Examples(AgilityInstanceClient instance)
    {
        UrlRedirectionSaveResult saved = await instance.UrlRedirections.SaveUrlRedirectionsAsync(
        [
            new UrlRedirection { UrlRedirectionID = 0, OriginUrl = "/old-page", DestinationUrl = "/new-page", HttpCode = 301 },
            new UrlRedirection { UrlRedirectionID = 0, OriginUrl = "/promo", DestinationUrl = "https://example.com/sale", HttpCode = 302 },
        ]);
        UrlRedirectionDeleteResult deleted = await instance.UrlRedirections.DeleteUrlRedirectionsAsync([12, 13]);

        await using (var export = await instance.UrlRedirections.ExportUrlRedirectionsAsync())
        await using (var file = File.Create("redirections.xlsx"))
            await export.CopyToAsync(file);

        await using var edited = File.OpenRead("redirections.xlsx");
        UrlRedirectionSaveResult imported = await instance.UrlRedirections.ImportUrlRedirectionsAsync(edited);
    }
}

internal static class UsersGuide
{
    public static async Task Examples(AgilityManagementClient client, AgilityInstanceClient instance, int editorRoleId)
    {
        ServerUser me = await client.Users.GetCurrentUserAsync();
        foreach (var site in me.WebsiteAccess ?? [])
            Console.WriteLine($"{site.WebsiteName}: {site.Guid}");

        List<WebsiteUser> users = await instance.Users.GetUsersAsync();
        InstanceUser user = await instance.Users.SaveUserAsync("editor@example.com",
            [new InstanceRole { RoleID = editorRoleId }], firstName: "Sam", lastName: "Lee");
        await instance.Users.DeleteUserAsync(user.UserID);

        var created = await client.PersonalAccessTokens.CreateTokenAsync(new PersonalAccessTokenRequest
        {
            Name = "nightly-sync",
            ExpiryDate = DateTime.UtcNow.AddYears(1),
        });
        PersonalAccessTokenListResponse mine = await client.PersonalAccessTokens.GetTokensAsync();
        await client.PersonalAccessTokens.UpdateTokenAsync(created.TokenID, new PersonalAccessTokenUpdateRequest { Enabled = false });
        await client.PersonalAccessTokens.RevokeTokenAsync(created.TokenID);

        string fetchKey = await instance.GetFetchApiKeyAsync();
        string previewKey = await instance.GetPreviewApiKeyAsync();
        AllTypesResponse types = await client.Types.GetAllTypesAsync();
    }
}
