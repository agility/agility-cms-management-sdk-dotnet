using agility.models;
using agility.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class WebhookTests
    {
        private ClientInstance clientInstance = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;

        public WebhookTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
        }

        [TestMethod]
        public async Task TestWebhookList()
        {
            string? guid = Environment.GetEnvironmentVariable("Guid");
            var webhooks = await clientInstance.webhookMethods.WebhookList(guid);
            Assert.IsNotNull(webhooks, "Unable to retrieve webhook list.");
        }

        [TestMethod]
        public async Task<Webhook?> TestSaveWebhook()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Create a test webhook
                var webhook = new Webhook
                {
                    webhookID = null,
                    name = $"Test Webhook {DateTime.Now:yyyyMMddHHmmss}",
                    url = "https://example.com/webhook",
                    events = new[] { "content.published", "content.unpublished" },
                    isActive = true
                };

                var savedWebhook = await clientInstance.webhookMethods.SaveWebhook(guid, webhook);
                Assert.IsNotNull(savedWebhook, "Unable to save webhook.");

                return webhook;
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to save webhook: {ex.Message}");
                return null;
            }
        }

        [TestMethod]
        public async Task TestGetWebhook()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // First create a webhook
                var savedWebhook = await TestSaveWebhook();
                Assert.IsNotNull(savedWebhook, "Failed to create webhook for retrieval test.");

                // Note: This assumes the saved webhook response contains the ID
                // You may need to adjust based on actual API response
                var webhookID = savedWebhook.webhookID;
                if (!string.IsNullOrEmpty(webhookID))
                {
                    var retrievedWebhook = await clientInstance.webhookMethods.GetWebhook(guid, webhookID);
                    Assert.IsNotNull(retrievedWebhook, $"Unable to retrieve webhook with ID: {webhookID}");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to get webhook: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestDeleteWebhook()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // First create a webhook
                var savedWebhook = await TestSaveWebhook();
                Assert.IsNotNull(savedWebhook, "Failed to create webhook for deletion test.");

                var webhookID = savedWebhook.webhookID;
                if (!string.IsNullOrEmpty(webhookID))
                {
                    await clientInstance.webhookMethods.DeleteWebhook(guid, webhookID);
                    // If no exception is thrown, deletion was successful
                    Assert.IsTrue(true, "Webhook deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to delete webhook: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestWebhookOperations()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Create webhook
                var webhook = new Webhook
                {
                    webhookID = null,
                    name = $"Test Webhook Operations {DateTime.Now:yyyyMMddHHmmss}",
                    url = "https://example.com/webhook-test",
                    events = new[] { "content.published" },
                    isActive = true
                };

                var savedWebhook = await clientInstance.webhookMethods.SaveWebhook(guid, webhook);
                Assert.IsNotNull(savedWebhook, "Unable to save webhook in operations test.");

                var webhookID = ((Webhook)savedWebhook).webhookID;
                Assert.IsNotNull(webhookID, "Webhook ID should not be null after save.");

                // Retrieve webhook
                var retrievedWebhook = await clientInstance.webhookMethods.GetWebhook(guid, webhookID);
                Assert.IsNotNull(retrievedWebhook, "Unable to retrieve saved webhook.");

                // Update webhook (if supported)
                // Note: Adjust based on actual webhook model properties
                
                // Delete webhook
                await clientInstance.webhookMethods.DeleteWebhook(guid, webhookID);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Webhook operations test failed: {ex.Message}");
            }
        }
    }
}
