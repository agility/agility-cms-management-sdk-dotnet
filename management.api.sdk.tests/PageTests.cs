using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using agility.utils;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class PageTests
    {
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;
        ClientInstance clientInstance = null;
        ContentTests contentTests = null;   
        public PageTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
            contentTests = new ContentTests();
        }

        private async Task<Model?> CreateModel()
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            Model model = new Model();
            model.id = 0;
            model.lastModifiedDate = DateTime.Now;
            string format = "Mddyyyyhhmmsstt";
            string modelDate = DateTime.Now.ToString(format);
            model.displayName = $"Test_PageModel {modelDate}";
            model.referenceName = $"TestPageModel{modelDate}";
            model.description = "Model created to test SDK";
            model.allowTagging = false;
            model.contentDefinitionTypeName = null;
            model.lastModifiedBy = null;

            var modelField = new ModelField();
            modelField.name = "TypeText";
            modelField.label = "Type Text";
            modelField.type = "Text";
            modelField.description = "Exclusive model for the Test project on SDK";
            modelField.labelHelpDescription = "Field Type Text";
            modelField.itemOrder = 0;
            modelField.designerOnly = false;
            modelField.isDataField = true;
            modelField.editable = true;
            modelField.hiddenField = false;
            modelField.settings.Add("Required", "False");
            modelField.settings.Add("Length", "");
            modelField.settings.Add("DefaultValue", "");
            modelField.settings.Add("Unique", "False");
            modelField.settings.Add("CopyAcrossAllLanguages", "False");
            modelField.settings.Add("DefaultValue-en-us", "");

            model.fields = new List<ModelField?>();
            model.fields.Add(modelField);
            var retModel = await clientInstance.modelMethods.SaveModel(model, guid);

           
            return retModel;
        }

        private async Task<Container> CreateContainer(Model model)
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            var container = new Container();
            container.ContentViewID = 0;
            container.ContentDefinitionID = model.id;
            container.ContentDefinitionTypeID = 1;
            string format = "Mddyyyyhhmmsstt";
            string modelDate = DateTime.Now.ToString(format);
            container.ContentViewName = $"Test_PageContainer {modelDate}";
            container.ReferenceName = $"TestPageContainer{modelDate}";
            container.IsShared = false;
            container.IsDynamicPageList = true;
            container.ContentViewCategoryID = null;
            var retcontainer = await clientInstance.containerMethods.SaveContainer(container, guid);
            return retcontainer;
        }

        [TestMethod]
        public async Task<int?> SavPage()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                var model = await CreateModel();
                Assert.IsNotNull(model, "Unable to create model");

                var container = await CreateContainer(model);
                Assert.IsNotNull(container, "Unable to create container");

                var contentObject = contentTests.GetContentObject(container);
                Assert.IsNotNull(contentObject, "Unable to create content object");

                var contentStr = await clientInstance.contentMethods.SaveContentItem(contentObject, guid, locale);
                int contentId = Convert.ToInt32(contentStr);
                if (contentId < 1)
                {
                    Assert.Fail("Content not created. Failing the test");
                }

                var pageItem = GetPageObject(model, contentId);

                var page = await clientInstance.pageMethods.SavePage(pageItem, guid, locale, -1, -1);
                Assert.IsNotNull(page, $"Unable to create page for model referenceName {model.referenceName} and contentID {contentObject.contentID}");

                var pageID = Convert.ToInt32(page);
                if (pageID < 1)
                {
                    Assert.IsNotNull(pageID, $"Page not created for model referenceName {model.referenceName} and contentID {contentObject.contentID}");
                }
                return pageID;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            return null;
        }

        private PageItem GetPageObject(Model model, int contentID)
        {
            var pageItem = new PageItem();
            pageItem.pageID = -1;
            pageItem.channelID = 1;
            pageItem.releaseDate = null;
            pageItem.pullDate = null;
            var guid = Guid.NewGuid();
            pageItem.name = $"sdk-page-{guid}";
            pageItem.path = string.Empty;
            pageItem.title = "Page for SDK";
            pageItem.menuText = "Page Menu for SDK";
            pageItem.pageType = "static";
            pageItem.templateName = "Main Template";
            pageItem.redirectUrl = string.Empty;
            pageItem.securePage = false;
            pageItem.excludeFromOutputCache = false;
            pageItem.visible = new PageVisible();
            pageItem.visible.menu = true;
            pageItem.visible.sitemap = true;
            pageItem.seo = new SeoProperties();
            pageItem.seo.metaDescription = "Just to test SDK";
            pageItem.seo.metaKeywords = "SDK";
            pageItem.seo.metaHTML = String.Empty;
            pageItem.seo.menuVisible = true;
            pageItem.seo.sitemapVisible = true;
            pageItem.scripts = new PageScripts();
            pageItem.scripts.excludedFromGlobal = false;
            pageItem.scripts.top = null;
            pageItem.scripts.bottom = null;

            pageItem.zones = new Dictionary<string?, List<PageModule?>>();

            PageModule pageModule = new PageModule();
            pageModule.module = model.referenceName;
            dynamic content = new System.Dynamic.ExpandoObject();
            content.contentId = contentID;
            pageModule.item = content;

            List<PageModule> pageModules = new List<PageModule>();
            pageModules.Add(pageModule);

            pageItem.zones.Add("MainContentZone", pageModules);
            return pageItem;
        }

        [TestMethod]
        public async Task<PageItem?> GetPage(int? pageID)
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            string? locale = Environment.GetEnvironmentVariable("Locale");
            var pageItem = await clientInstance.pageMethods.GetPage(pageID, guid, locale);
            Assert.IsNotNull(pageItem, $"Unable to find Page for page id {pageID}.");

            return pageItem;
        }

        [TestMethod]
        public async Task PageOperations()
        {
            try
            {

                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                var pageId = await SavPage();

                var pageItem = await GetPage(pageId);
                Assert.IsNotNull(pageItem, "Unable to retrieve content.");

                var publishPage = await clientInstance.pageMethods.PublishPage(pageId, guid, locale, "Publish Page");
                Assert.IsNotNull(publishPage, $"In:PublishPage: Unable to generate batch for the pageId: {pageId}");
                if (publishPage < 1)
                {
                    Assert.Fail($"In:PublishPage: Unable to generate batch for the pageID: {pageId}. Negative value of the batchID");
                }

                var unpublishPage = await clientInstance.pageMethods.UnPublishPage(pageId, guid, locale, "Un-Publish Page");
                Assert.IsNotNull(unpublishPage, $"In:UnPublishPage: Unable to generate batch for the pageId: {pageId}");
                if (unpublishPage < 1)
                {
                    Assert.Fail($"In:UnPublishPage: Unable to generate batch for the pageID: {pageId}. Negative value of the batchID");
                }

                var pageApprovalReq = await clientInstance.pageMethods.PageRequestApproval(pageId, guid, locale, "Request for Page Approval");
                Assert.IsNotNull(pageApprovalReq, $"In:PageApprovalRequest: Unable to generate batch for the pageId: {pageId}");
                if (pageApprovalReq < 1)
                {
                    Assert.Fail($"In:PageApprovalRequest: Unable to generate batch for the pageID: {pageId}. Negative value of the batchID");
                }

                var approvePage = await clientInstance.pageMethods.ApprovePage(pageId, guid, locale, "Page Approval");
                Assert.IsNotNull(approvePage, $"In:ApprovePage: Unable to generate batch for the pageId: {pageId}");
                if (approvePage < 1)
                {
                    Assert.Fail($"In:ApprovePage: Unable to generate batch for the pageID: {pageId}. Negative value of the batchID");
                }

                var declinePage = await clientInstance.pageMethods.DeclinePage(pageId, guid, locale, "Page Declined");
                Assert.IsNotNull(declinePage, $"In:DeclinePage: Unable to generate batch for the pageId: {pageId}");
                if (declinePage < 1)
                {
                    Assert.Fail($"In:DeclinePage: Unable to generate batch for the pageID: {pageId}. Negative value of the batchID");
                }

                var deletePage = await clientInstance.pageMethods.DeletePage(pageId, guid, locale, "Delete Page");
                Assert.IsNotNull(deletePage, $"In:DeletePage: Unable to generate batch for the pageId: {pageId}");
                if (deletePage < 1)
                {
                    Assert.Fail($"In:DeletePage: Unable to generate batch for the pageID: {pageId}. Negative value of the batchID");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public async Task GetSiteMap()
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            string? locale = Environment.GetEnvironmentVariable("Locale");
            var sitemap = await clientInstance.pageMethods.GetSiteMap(guid, locale);
            Assert.IsNotNull(sitemap, "Unable to get the sitemap");
        }
    }
}
