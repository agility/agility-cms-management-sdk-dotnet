using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp.Serializers.Json;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace management.api.sdk.tests
{
    /// <summary>
    /// Offline tests for how page template zones are serialized. A zone's defaultModules list replaces its
    /// default components on save, so it must be omitted unless the caller has set it.
    /// </summary>
    [TestClass]
    public class ContentSectionDefinitionSerializationTests
    {
        private const string PageTemplateResponse = @"{
            ""pageTemplateID"": 12,
            ""pageTemplateName"": ""Main Template"",
            ""contentSectionDefinitions"": [
                {
                    ""pageItemTemplateID"": 34,
                    ""pageItemTemplateName"": ""Main Zone"",
                    ""pageItemTemplateReferenceName"": ""MainZone"",
                    ""defaultModules"": [
                        {
                            ""title"": ""Rich Text"",
                            ""pageItemTemplateDefaultModuleID"": 56,
                            ""contentDefinitionID"": 78,
                            ""pageItemTemplateID"": 34,
                            ""autoCreate"": true
                        }
                    ]
                }
            ]
        }";

        // Same serializer RestSharp uses for AddJsonBody in ClientInstance.ExecutePost.
        private static string? SerializeRequestBody(object data) => new SystemTextJsonSerializer().Serialize(data);

        // Same options PageMethods uses to read page templates.
        private static PageModel? DeserializeResponse(string json) =>
            JsonSerializer.Deserialize<PageModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        [TestMethod]
        public void NewZoneOmitsDefaultModulesAndSharedModules()
        {
            var pageModel = new PageModel
            {
                PageTemplateName = "Main Template",
                ContentSectionDefinitions = new List<ContentSectionDefinition?>
                {
                    new ContentSectionDefinition { PageItemTemplateName = "Main Zone" }
                }
            };

            var body = SerializeRequestBody(pageModel);

            Assert.IsNotNull(body);
            Assert.IsFalse(body.Contains("defaultModules", StringComparison.OrdinalIgnoreCase), body);
            Assert.IsFalse(body.Contains("sharedModules", StringComparison.OrdinalIgnoreCase), body);
        }

        [TestMethod]
        public void DefaultModulesRoundTripFromReadToSave()
        {
            var pageModel = DeserializeResponse(PageTemplateResponse);

            var zone = pageModel?.ContentSectionDefinitions?[0];
            Assert.IsNotNull(zone?.DefaultModules);
            Assert.AreEqual(1, zone!.DefaultModules!.Count);
            Assert.AreEqual(78, zone.DefaultModules[0].ContentDefinitionID);

            var body = SerializeRequestBody(pageModel!);
            var sentZone = JsonDocument.Parse(body!).RootElement.GetProperty("contentSectionDefinitions")[0];
            var sentDefaults = sentZone.GetProperty("defaultModules");

            Assert.AreEqual(1, sentDefaults.GetArrayLength());
            Assert.AreEqual(56, sentDefaults[0].GetProperty("pageItemTemplateDefaultModuleID").GetInt32());
            Assert.AreEqual(78, sentDefaults[0].GetProperty("contentDefinitionID").GetInt32());
        }

        [TestMethod]
        public void ExplicitEmptyDefaultModulesSerializesAsEmptyArray()
        {
            var zone = new ContentSectionDefinition { DefaultModules = new List<ContentSectionDefaultModule>() };

            var body = SerializeRequestBody(zone);
            var sentDefaults = JsonDocument.Parse(body!).RootElement.GetProperty("defaultModules");

            Assert.AreEqual(JsonValueKind.Array, sentDefaults.ValueKind);
            Assert.AreEqual(0, sentDefaults.GetArrayLength());
        }
    }
}
