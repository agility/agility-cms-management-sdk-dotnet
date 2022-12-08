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
        ClientInstance clientInstance = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;
        ContentTests contentTests = null;

        public ContainerTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
            contentTests = new ContentTests();
        }

        [TestMethod]
        public async Task ConatinerOperations()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
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

                var retcontainer = await clientInstance.containerMethods.SaveContainer(container,guid);
                Assert.IsNotNull(retcontainer, $"Unable to create container for reference name {container.ReferenceName}");

                var contByID = await clientInstance.containerMethods.GetContainerById(retcontainer.ContentViewID, guid);
                Assert.IsNotNull(contByID, $"Unable to retrieve container for containerID {retcontainer.ContentViewID}");

                var contByRef = await clientInstance.containerMethods.GetContainerByReferenceName(retcontainer.ReferenceName, guid);
                Assert.IsNotNull(contByRef, $"Unable to retrieve container for reference name {retcontainer.ReferenceName}");

                var contSecurity = await clientInstance.containerMethods.GetContainerSecurity(retcontainer.ContentViewID, guid);
                Assert.IsNotNull(contSecurity, $"Unable to retrieve security for container for containerID {retcontainer.ContentViewID}");

                var containerList = await clientInstance.containerMethods.GetContainerList(guid);
                Assert.IsNotNull(containerList, $"Unable to retrieve container list");

                var notificationList = await clientInstance.containerMethods.GetNotificationList(retcontainer.ContentViewID, guid);
                Assert.IsNotNull(notificationList, $"Unable to retrieve notifications for container for containerID {retcontainer.ContentViewID}");

                var deleteContainer = await clientInstance.containerMethods.DeleteContainer(retcontainer.ContentViewID, guid);
                Assert.IsNotNull(deleteContainer, $"Unable to delete container for containerID {retcontainer.ContentViewID}");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
           
        }
    }
}
