using agility.models;
using Microsoft.Extensions.Options;
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
        ContentMethods contentMethods = null;
        ModelMethods modelMethods = null;
        ContainerMethods containerMethods = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;
        public ContentTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            contentMethods = new ContentMethods(_options);
            modelMethods = new ModelMethods(_options);
            containerMethods = new ContainerMethods(_options);
        }

        [TestMethod]
        public async Task<ContentItem?> GetContentItem(int contentID)
        {
            var contentItem = await contentMethods.GetContentItem(contentID);
            Assert.IsNotNull(contentItem, $"Unable to find Content item for content id {contentID}.");

            return contentItem;
        }

        [TestMethod]
        public async Task<Model?> SaveContentModel()
        {
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
            var retModel = await modelMethods.SaveModel(model);

            Assert.IsNotNull(retModel, $"Unable to create model for reference name {model.referenceName}");

            return retModel;
        }

        [TestMethod]
        public async Task<Container?> SaveContainer()
        {
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
            //container.ContentDefinitionType = 1;
            var retcontainer = await containerMethods.SaveContainer(container);

            Assert.IsNotNull(retcontainer, $"Unable to create container for reference name {container.ReferenceName}");
            return retcontainer;
        }

        [TestMethod]
        public async Task<int?> SaveContent()
        {
            try
            {
                var container = await SaveContainer();

                var contentItem = GetContentObject(container);

                var contentStr = await contentMethods.SaveContentItem(contentItem);
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

        private ContentItem GetContentObject(Container container)
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
                var contentId = await SaveContent();

                var content = await GetContentItem((int)contentId);
                Assert.IsNotNull(content, "Unable to retrieve content.");

                var publishContent = await contentMethods.PublishContent(contentId, "Publish Content");
                Assert.IsNotNull(publishContent, $"In:PublishContent: Unable to generate batch for the contentID: {contentId}");

                var unpublishContent = await contentMethods.UnPublishContent(contentId, "Un-Publish Content");
                Assert.IsNotNull(unpublishContent, $"In:UnPublishContent: Unable to generate batch for the contentID: {contentId}");

                var contentRequestApproval = await contentMethods.ContentRequestApproval(contentId, "Request for content approval");
                Assert.IsNotNull(contentRequestApproval, $"In:ContentRequestApproval: Unable to generate batch for the contentID: {contentId}");

                var approveContent = await contentMethods.ApproveContent(contentId, "Content Approved");
                Assert.IsNotNull(approveContent, $"In:ApproveContent: Unable to generate batch for the contentID: {contentId}");

                var declineContent = await contentMethods.DeclineContent(contentId, "Content Declined");
                Assert.IsNotNull(declineContent, $"In:DeclineContent: Unable to generate batch for the contentID: {contentId}");

                var deleteContent = await contentMethods.DeleteContent(contentId, "Delete Content");
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
                var container = await SaveContainer();
                List<ContentItem> contentItems = new List<ContentItem>();

                for (int i = 0; i < 2; i++)
                {
                    var contentItem = GetContentObject(container);
                    contentItems.Add(contentItem);
                }

                var items = await contentMethods.SaveContentItems(contentItems);

                if (items.Count < 1)
                {
                    Assert.Fail("Unable to save multiple content.");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
           
        }
      
    }
}