using agility.models;
using System.Text.Json;

namespace management.api.sdk
{
    /// <summary>
    /// Methods for webhook management.
    /// </summary>
    public class WebhookMethods
    {
        private Options _options;
        ExecuteMethods executeMethods;

        public WebhookMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Lists all webhooks for the instance.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="take">Number of webhooks to retrieve.</param>
        /// <param name="token">Optional token parameter.</param>
        /// <returns>List of webhooks.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<object?> WebhookList(string guid, int take = 20, string? token = null)
        {
            try
            {
                var apiPath = "/webhook/list";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to get the webhook list. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var webhooks = JsonSerializer.Deserialize<object>(response.Result.Content, options);

                return webhooks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Saves or updates a webhook.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="webhook">The webhook object to save.</param>
        /// <returns>The saved webhook.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<object?> SaveWebhook(string guid, Webhook webhook)
        {
            try
            {
                var apiPath = "/webhook";
                var response = executeMethods.ExecutePost(apiPath, guid, webhook, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to save webhook. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var savedWebhook = JsonSerializer.Deserialize<object>(response.Result.Content, options);

                return savedWebhook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets a specific webhook by ID.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="webhookID">The webhook ID.</param>
        /// <returns>The requested webhook.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<object?> GetWebhook(string guid, string webhookID)
        {
            try
            {
                var apiPath = $"/webhook/{webhookID}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve webhook for webhookID: {webhookID}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var webhook = JsonSerializer.Deserialize<object>(response.Result.Content, options);

                return webhook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Deletes a webhook by ID.
        /// </summary>
        /// <param name="guid">Current website guid.</param>
        /// <param name="webhookID">The webhook ID to delete.</param>
        /// <exception cref="ApplicationException"></exception>
        public async Task DeleteWebhook(string guid, string webhookID)
        {
            try
            {
                var apiPath = $"/webhook/{webhookID}";
                var response = executeMethods.ExecuteDelete(apiPath, guid, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete webhook for webhookID: {webhookID}. Additional Details: {response.Result.Content}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
