using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using agility.utils;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class ContentLoadTests
    {
        ContentMethods contentMethods = null;
        ContentTests contentTests = null;
        ContainerMethods containerMethods = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;

        public ContentLoadTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            contentMethods = new ContentMethods(_options);
            contentTests = new ContentTests();
            containerMethods = new ContainerMethods(_options);
        }


        [TestMethod]
        public async Task SaveMultipleContents()
        {
            try
            {
                List<ContentItem> contentItems = new List<ContentItem>();
                var container = await contentTests.SaveContainer();
                for (int i = 0; i < 250; i++)
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
                    int contentID = 0;
                    bool isValid = int.TryParse(item, out contentID);

                    if (isValid)
                    {
                        var deleteContent = await contentMethods.DeleteContent(contentID, "Delete Content");

                        int id = 0;
                        bool valid = false;
                        valid = int.TryParse(deleteContent, out id);
                        if (!valid)
                        {
                            Assert.Fail($"In:SaveMultipleContents: Unable to generate batch for the contentID: {contentID}. Negative value of the batchID");
                        }
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
                for (int i = 0; i < 250; i++)
                {
                    contentItems.Add(contentTests.GetContentObject(container));
                }
                var items = await contentMethods.SaveContentItems(contentItems);
                Assert.IsNotNull(items, "Processing the batches in background.");

                if (items.Count != contentItems.Count)
                {
                    Assert.Fail("Unable to save multiple contents.");
                }

               /* foreach (var item in items)
                {
                    int contentID = 0;
                    bool isValid = int.TryParse(item, out contentID);

                    if (isValid)
                    {
                        var deleteContent = await contentMethods.DeleteContent(contentID, "Delete Content");

                        int id = 0;
                        bool valid = false;
                        valid = int.TryParse(deleteContent, out id);
                        if (!valid)
                        {
                            Assert.Fail($"In:SaveMultipleContents: Unable to generate batch for the contentID: {contentID}. Negative value of the batchID");
                        }
                    }
                }

                var containerStr = await containerMethods.DeleteContainer(container.ContentViewID);

                Assert.IsNotNull(containerStr, "Unable to delete container.");*/

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
