using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using agility.utils;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class ContentTests
    {
        ClientInstance clientInstance = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;
        public ContentTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
        }

        [TestMethod]
        public async Task<ContentItem?> GetContentItem(int contentID)
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            string? locale = Environment.GetEnvironmentVariable("Locale");
            var contentItem = await clientInstance.contentMethods.GetContentItem(contentID, guid, locale);
            Assert.IsNotNull(contentItem, $"Unable to find Content item for content id {contentID}.");

            return contentItem;
        }

        [TestMethod]
        public async Task<Model?> SaveContentModel()
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            Model model = new Model();
            model.id = 0;
            model.lastModifiedDate = DateTime.Now;
            string format = "Mddyyyyhhmmsstt";
            string modelDate = DateTime.Now.ToString(format);
            model.displayName = $"Test_Model {modelDate}";
            model.referenceName = $"TestModel{modelDate}";
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

            Assert.IsNotNull(retModel, $"Unable to create model for reference name {model.referenceName}");

            return retModel;
        }

        [TestMethod]
        public async Task<Container?> SaveContainer()
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            var model = await SaveContentModel();
            var container = new Container();
            container.ContentViewID = 0;
            container.ContentDefinitionID = model.id;
            container.ContentDefinitionTypeID = 1;
            string format = "Mddyyyyhhmmsstt";
            string modelDate = DateTime.Now.ToString(format);
            container.ContentViewName = $"Test_Container {modelDate}";
            container.ReferenceName = $"TestContainer{modelDate}";
            container.IsShared = false;
            container.IsDynamicPageList = true;
            container.ContentViewCategoryID = null;
            var retcontainer = await clientInstance.containerMethods.SaveContainer(container, guid);

            Assert.IsNotNull(retcontainer, $"Unable to create container for reference name {container.ReferenceName}");
            return retcontainer;
        }

        [TestMethod]
        public async Task<int?> SaveContent()
        {
            try
            {

                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                var container = await SaveContainer();

                var contentItem = GetContentObject(container);

                var contentStr = await clientInstance.contentMethods.SaveContentItem(contentItem, guid, locale);
                Assert.IsNotNull(contentStr, $"Unable to create content.");

                int contentID = Convert.ToInt32(contentStr);
                return contentID;
            }

            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            return null;
        }

        public ContentItem GetContentObject(Container container)
        {
            var contentItem = new ContentItem();
            contentItem.contentID = -1;

            ContentItemProperties properties = new ContentItemProperties();
            properties.state = 1;
            properties.modified = DateTime.Now;
            properties.versionID = 0;
            properties.referenceName = container.ReferenceName;
            properties.definitionName = container.ContentDefinitionName;
            properties.itemOrder = 0;
            contentItem.properties = properties;

            Dictionary<string, object> fields = new Dictionary<string, object>();
            fields.Add("typetext", "Test text for Content: From SDK");
            contentItem.fields = fields;

            return contentItem;
        }

        [TestMethod]
        public async Task ContentOperations()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                var contentId = await SaveContent();

                var content = await GetContentItem((int)contentId);
                Assert.IsNotNull(content, "Unable to retrieve content.");
                int id = 0;
                var publishContent = await clientInstance.contentMethods.PublishContent(contentId, guid, locale, "Publish Content");
                Assert.IsNotNull(publishContent, $"In:PublishContent: Unable to generate batch for the contentID: {contentId}");
               
                var unpublishContent = await clientInstance.contentMethods.UnPublishContent(contentId, guid, locale, "Un-Publish Content");
                Assert.IsNotNull(unpublishContent, $"In:UnPublishContent: Unable to generate batch for the contentID: {contentId}");

                var contentRequestApproval = await clientInstance.contentMethods.ContentRequestApproval(contentId, guid, locale, "Request for content approval");
                Assert.IsNotNull(contentRequestApproval, $"In:ContentRequestApproval: Unable to generate batch for the contentID: {contentId}");

                var approveContent = await clientInstance.contentMethods.ApproveContent(contentId, guid, locale, "Content Approved");
                Assert.IsNotNull(approveContent, $"In:ApproveContent: Unable to generate batch for the contentID: {contentId}");

                var declineContent = await clientInstance.contentMethods.DeclineContent(contentId, guid, locale, "Content Declined");
                Assert.IsNotNull(declineContent, $"In:DeclineContent: Unable to generate batch for the contentID: {contentId}");

                var deleteContent = await clientInstance.contentMethods.DeleteContent(contentId, guid, locale, "Delete Content");
                Assert.IsNotNull(deleteContent, $"In:DeleteContent: Unable to generate batch for the contentID: {contentId}");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

        }

        [TestMethod]
        public async Task SaveContents()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                var container = await SaveContainer();
                List<ContentItem> contentItems = new List<ContentItem>();

                for (int i = 0; i < 2; i++)
                {
                    var contentItem = GetContentObject(container);
                    contentItems.Add(contentItem);
                }

                var items = await clientInstance.contentMethods.SaveContentItems(contentItems, guid, locale);

                if (items.Count < 1)
                {
                    Assert.Fail("Unable to save multiple content.");
                }

                foreach (var item in items)
                {
                    bool isValid = isInteger(item);

                    if (isValid)
                    {
                        var deleteContent = await clientInstance.contentMethods.DeleteContent(Convert.ToInt32(item), guid, locale, "Delete Content");
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
           
        }

        [TestMethod]
        public async Task TestGetContentItems()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                
                // First create a container to test with
                var container = await SaveContainer();
                Assert.IsNotNull(container, "Unable to create container for GetContentItems test.");

                // Test getting content items with various parameters
                var contentItems = await clientInstance.contentMethods.GetContentItems(
                    container.ReferenceName,
                    guid,
                    locale,
                    filter: "",
                    fields: "",
                    sortDirection: "asc",
                    sortField: "contentID",
                    take: 10,
                    skip: 0
                );

                Assert.IsNotNull(contentItems, "GetContentItems should return a result.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"GetContentItems test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetContentList()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                
                // Create a container to test with
                var container = await SaveContainer();
                Assert.IsNotNull(container, "Unable to create container for GetContentList test.");

                // Test getting content list with filter
                var contentList = await clientInstance.contentMethods.GetContentList(
                    container.ReferenceName,
                    guid,
                    locale,
                    take: 10,
                    filterObject: null
                );

                Assert.IsNotNull(contentList, "GetContentList should return a result.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"GetContentList test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetContentHistory()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                
                // Create content first
                var contentId = await SaveContent();
                Assert.IsNotNull(contentId, "Unable to create content for history test.");

                // Get content history
                var history = await clientInstance.contentMethods.GetContentHistory(
                    locale,
                    guid,
                    (int)contentId,
                    take: 10,
                    skip: 0
                );

                Assert.IsNotNull(history, "GetContentHistory should return a result.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"GetContentHistory test failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetContentComments()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                
                // Create content first
                var contentId = await SaveContent();
                Assert.IsNotNull(contentId, "Unable to create content for comments test.");

                // Get content comments
                var comments = await clientInstance.contentMethods.GetContentComments(
                    locale,
                    guid,
                    (int)contentId,
                    take: 10,
                    skip: 0
                );

                Assert.IsNotNull(comments, "GetContentComments should return a result.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"GetContentComments test failed: {ex.Message}");
            }
        }
      
        public bool isInteger(object value)
        {
            return value is int;
        }
      
    }
}