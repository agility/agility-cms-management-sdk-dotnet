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
        ModelMethods modelMethods = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;
        ContentTests contentTests = null;

        public ModelTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            modelMethods = new ModelMethods(_options);
            contentTests = new ContentTests();
        }

        [TestMethod]
        public async Task ModelOperations()
        {
            try
            {
                var model = await contentTests.SaveContentModel();
                Assert.IsNotNull(model, $"Unable to create model.");

                var modelByID = await modelMethods.GetContentModel(model.id);
                Assert.IsNotNull(modelByID, $"Unable to retrieve model for id {model.id}");

                var contentModules = await modelMethods.GetContentModules(true, true);
                Assert.IsNotNull(contentModules, $"Unable to retrieve content modules");

                var pageModules = await modelMethods.GetPageModules(true);
                Assert.IsNotNull(contentModules, $"Unable to retrieve page modules");

                var deleteModel = await modelMethods.DeleteModel(model.id);
                Assert.IsNotNull(contentModules, $"Unable to delete model for id {model.id}");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
