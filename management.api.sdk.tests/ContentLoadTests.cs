using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using agility.utils;
using System.Threading.Tasks;
using System.Text.Json;

namespace management.api.sdk.tests
{
    [TestClass]
    public class ContentLoadTests
    {
        ContentTests contentTests = null;
        ClientInstance clientInstance = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;

        public ContentLoadTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
            contentTests = new ContentTests();
        }


        [TestMethod]
        public async Task SaveMultipleContents()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                List<ContentItem> contentItems = new List<ContentItem>();
                var container = await contentTests.SaveContainer();
                for (int i = 0; i < 250; i++)
                {
                    contentItems.Add(contentTests.GetContentObject(container));
                }
                var items = await clientInstance.contentMethods.SaveContentItems(contentItems, guid, locale);
                Assert.IsNotNull(items, "Processing the batches in background.");

                if(items.Count != contentItems.Count)
                {
                    Assert.Fail("Unable to save multiple contents.");
                }

                foreach (var item in items)
                {
                    bool isValid = isInteger(item);

                    if (isValid)
                    {
                        var deleteContent = await clientInstance.contentMethods.DeleteContent(Convert.ToInt32(item), guid, locale, "Delete Content");
                    }
                }

                var containerStr = await clientInstance.containerMethods.DeleteContainer(container.ContentViewID, guid);

                Assert.IsNotNull(containerStr, "Unable to delete container.");

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public async Task UpdateContents()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                string? locale = Environment.GetEnvironmentVariable("Locale");
                List<ContentItem> contentItems = new List<ContentItem>();
                var container = await contentTests.SaveContainer();
                for (int i = 0; i < 250; i++)
                {
                    contentItems.Add(contentTests.GetContentObject(container));
                }
                var items = await clientInstance.contentMethods.SaveContentItems(contentItems, guid, locale);
                Assert.IsNotNull(items, "Processing the batches in background.");

                if (items.Count != contentItems.Count)
                {
                    Assert.Fail("Unable to save multiple contents.");
                }

                List<ContentItem> updateItems = new List<ContentItem>();
                foreach (var item in items)
                {
                    bool isValid = isInteger(item);

                    if (isValid)
                    {
                        var contentItem = await clientInstance.contentMethods.GetContentItem(Convert.ToInt32(item), guid, locale);
                        contentItem.fields = new Dictionary<string, object>();
                        contentItem.fields.Add("typeText", "Updated Test text for Content: From SDK");
                        updateItems.Add(contentItem);
                    }
                }

 
                var updatedItems = await clientInstance.contentMethods.SaveContentItems(updateItems, guid, locale);

                Assert.IsNotNull(updatedItems, "Processing the batches in background.");

                foreach (var item in updatedItems)
                {
                    bool isValid = isInteger(item);

                    if (isValid)
                    {
                        var deleteContent = await clientInstance.contentMethods.DeleteContent(Convert.ToInt32(item), guid, locale, "Delete Content");
                    }
                }

                var containerStr = await clientInstance.containerMethods.DeleteContainer(container.ContentViewID, guid);

                Assert.IsNotNull(containerStr, "Unable to delete container.");

                var modelStr = await clientInstance.modelMethods.DeleteModel(container.ContentDefinitionID, guid);

                Assert.IsNotNull(modelStr, "Unable to delete container.");

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        public bool isInteger(object value)
        {
            return value is int;
        }
    }
}
