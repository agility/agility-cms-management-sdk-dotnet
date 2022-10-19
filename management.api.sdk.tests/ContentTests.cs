using agility.models;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class ContentTests
    {
        ContentMethods contentMethods = null;
        ModelMethods modelMethods = null;
        ContainerMethods containerMethods = null;
        private readonly agility.models.Options _options;
        private readonly AppSettings _appSettings;
        private readonly AuthMethods _authMethods;
        private TokenResponseData _tokenResponseData;
        public ContentTests()
        {
            _options = new agility.models.Options();
            _appSettings = new AppSettings();
            _options.guid = Environment.GetEnvironmentVariable("Guid");
            _options.locale = Environment.GetEnvironmentVariable("Locale");
            IOptions<AppSettings> appSettingsOptions = Microsoft.Extensions.Options.Options.Create<AppSettings>(_appSettings);
            _authMethods = new AuthMethods(_options, appSettingsOptions);
           
            modelMethods = new ModelMethods(_options);
            containerMethods = new ContainerMethods(_options);
        }

        private TokenResponseData GetTokenResponseDataAsync()
        {
            var tokenResponseData = _authMethods.GetCurrentToken(_options.guid);
            if (tokenResponseData != null)
            {
                _options.refresh_token = tokenResponseData.refresh_token;
            }
            else
            {
                _options.refresh_token = Environment.GetEnvironmentVariable("RefreshToken");
            }
            _tokenResponseData = _authMethods.GetAuthorizationToken(_options.guid);
            _options.token = _tokenResponseData.access_token;
            _options.refresh_token = _tokenResponseData.refresh_token;
            return _tokenResponseData;
        }

        [TestMethod]
        public async Task<ContentItem?> GetContentItem(int contentID)
        {
            var contentItem = await contentMethods.GetContentItem(contentID);
            Assert.IsNotNull(contentItem, $"Unable to find Content item for content id {contentID}.");

            if (!string.IsNullOrWhiteSpace(contentItem))
            {
                var content = JsonSerializer.Deserialize<ContentItem>(contentItem);
                return content;
            }
            return null;
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
            var modelStr = await modelMethods.SaveModel(model);

            Assert.IsNotNull(modelStr, $"Unable to create model for reference name {model.referenceName}");

            if (!string.IsNullOrWhiteSpace(modelStr))
            {
                model = new Model();
                model = JsonSerializer.Deserialize<Model>(modelStr);
                return model;
            }
            return null;
        }

        [TestMethod]
        public async Task<Container?> SaveContainer()
        {
            var model = await SaveContentModel();
            var container = new Container();
            container.ContentViewID = 0;
            container.ContentDefinitionID = model.id;
            container.ContentDefinitionTypeID = 0;
            string format = "Mddyyyyhhmmsstt";
            string modelDate = DateTime.Now.ToString(format);
            container.ContentViewName = $"Test_Container {modelDate}";
            container.ReferenceName = $"TestContainer{modelDate}";
            container.IsShared = false;
            container.IsDynamicPageList = true;
            container.ContentViewCategoryID = null;
            container.ContentDefinitionType = 1;
            var containerStr = await containerMethods.SaveContainer(container);

            Assert.IsNotNull(containerStr, $"Unable to create container for reference name {container.ReferenceName}");

            if (!string.IsNullOrWhiteSpace(containerStr))
            {
                container = new Container();
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                container = JsonSerializer.Deserialize<Container>(containerStr, options);
                return container;
            }
            return null;
        }

        [TestMethod]
        public async Task SaveContent()
        {
            var container = await SaveContainer();
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

            var contentStr = await contentMethods.SaveContentItem(contentItem);

            Assert.IsNotNull(contentStr, $"Unable to create content.");

            int contentID = Convert.ToInt32(contentStr);

            await GetContentItem(contentID);
        
        }

      
    }
}