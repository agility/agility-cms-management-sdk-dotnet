using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using agility.utils;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class ContainerTests
    {
        ContainerMethods containerMethods = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;
        ContentTests contentTests = null;

        public ContainerTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            containerMethods = new ContainerMethods(_options);
            contentTests = new ContentTests();
        }

        [TestMethod]
        public async Task ConatinerOperations()
        {
            try
            {
                var model = await contentTests.SaveContentModel();
                Assert.IsNotNull(model, $"Unable to create model.");

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

                var retcontainer = await containerMethods.SaveContainer(container);
                Assert.IsNotNull(retcontainer, $"Unable to create container for reference name {container.ReferenceName}");

                var contByID = await containerMethods.GetContainerById(retcontainer.ContentViewID);
                Assert.IsNotNull(contByID, $"Unable to retrieve container for containerID {retcontainer.ContentViewID}");

                var contByRef = await containerMethods.GetContainerByReferenceName(retcontainer.ReferenceName);
                Assert.IsNotNull(contByRef, $"Unable to retrieve container for reference name {retcontainer.ReferenceName}");

                var contSecurity = await containerMethods.GetContainerSecurity(retcontainer.ContentViewID);
                Assert.IsNotNull(contSecurity, $"Unable to retrieve security for container for containerID {retcontainer.ContentViewID}");

                var containerList = await containerMethods.GetContainerList();
                Assert.IsNotNull(containerList, $"Unable to retrieve container list");

                var notificationList = await containerMethods.GetNotificationList(retcontainer.ContentViewID);
                Assert.IsNotNull(notificationList, $"Unable to retrieve notifications for container for containerID {retcontainer.ContentViewID}");

                var deleteContainer = await containerMethods.DeleteContainer(retcontainer.ContentViewID);
                Assert.IsNotNull(deleteContainer, $"Unable to delete container for containerID {retcontainer.ContentViewID}");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
           
        }
    }
}
