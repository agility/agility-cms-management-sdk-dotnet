using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using agility.utils;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class ModelTests
    {
        ClientInstance clientInstance = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;
        ContentTests contentTests = null;

        public ModelTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
            contentTests = new ContentTests();
        }

        [TestMethod]
        public async Task ModelOperations()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                var model = await contentTests.SaveContentModel();
                Assert.IsNotNull(model, $"Unable to create model.");

                var modelByID = await clientInstance.modelMethods.GetContentModel(model.id, guid);
                Assert.IsNotNull(modelByID, $"Unable to retrieve model for id {model.id}");

                var contentModules = await clientInstance.modelMethods.GetContentModules(true, guid, true);
                Assert.IsNotNull(contentModules, $"Unable to retrieve content modules");

                var pageModules = await clientInstance.modelMethods.GetPageModules(guid, true);
                Assert.IsNotNull(contentModules, $"Unable to retrieve page modules");

                var deleteModel = await clientInstance.modelMethods.DeleteModel(model.id, guid);
                Assert.IsNotNull(contentModules, $"Unable to delete model for id {model.id}");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
