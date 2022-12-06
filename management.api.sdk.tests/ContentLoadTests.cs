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
        ContentMethods contentMethods = null;
        ContentTests contentTests = null;
        ContainerMethods containerMethods = null;
        ModelMethods modelMethods = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;

        public ContentLoadTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            contentMethods = new ContentMethods(_options);
            contentTests = new ContentTests();
            containerMethods = new ContainerMethods(_options);
            modelMethods = new ModelMethods(_options);
        }


        [TestMethod]
        public async Task SaveMultipleContents()
        {
            try
            {
                List<ContentItem> contentItems = new List<ContentItem>();
                var container = await contentTests.SaveContainer();
                for (int i = 0; i < 10; i++)
                {
                    contentItems.Add(contentTests.GetContentObject(container));
                }
                var items = await contentMethods.SaveContentItems(contentItems);
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
                        var deleteContent = await contentMethods.DeleteContent(Convert.ToInt32(item), "Delete Content");
                    }
                }

                var containerStr = await containerMethods.DeleteContainer(container.ContentViewID);

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
                List<ContentItem> contentItems = new List<ContentItem>();
                var container = await contentTests.SaveContainer();
                for (int i = 0; i < 10; i++)
                {
                    contentItems.Add(contentTests.GetContentObject(container));
                }
                var items = await contentMethods.SaveContentItems(contentItems);
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
                        var contentItem = await contentMethods.GetContentItem(Convert.ToInt32(item));
                        contentItem.fields = new Dictionary<string, object>();
                        contentItem.fields.Add("typeText", "Updated Test text for Content: From SDK");
                        updateItems.Add(contentItem);
                    }
                }

 
                var updatedItems = await contentMethods.SaveContentItems(updateItems);

                Assert.IsNotNull(updatedItems, "Processing the batches in background.");

                foreach (var item in updatedItems)
                {
                    bool isValid = isInteger(item);

                    if (isValid)
                    {
                        var deleteContent = await contentMethods.DeleteContent(Convert.ToInt32(item), "Delete Content");
                    }
                }

                var containerStr = await containerMethods.DeleteContainer(container.ContentViewID);

                Assert.IsNotNull(containerStr, "Unable to delete container.");

                var modelStr = await modelMethods.DeleteModel(container.ContentDefinitionID);

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
