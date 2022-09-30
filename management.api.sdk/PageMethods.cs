using agility.models;
using RestSharp;
namespace management.api.sdk
{
    public class PageMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        public PageMethods(string? baseAddress, string? guid)
        {
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(baseAddress, guid);
        }
        public async Task<string?> GetSiteMap(string? locale)
        {
            try
            {
                var request = new RestRequest($"/{locale}/sitemap");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetPage(string? locale, int? pageID)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> PublishPage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/publish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> UnPublishPage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/unpublish?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeletePage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Delete).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> ApprovePage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/approve?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeclinePage(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/decline?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> PageRequestApproval(string? locale, int? pageID, string? comments = null)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page/{pageID}/request-approval?comments={comments}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> SavePage(string? locale, PageItem? pageItem, int? parentPageID = -1, int? placeBeforePageItemID = -1)
        {
            try
            {
                var request = new RestRequest($"/{locale}/page?parentPageID={parentPageID}&placeBeforePageItemID={placeBeforePageItemID}");
                request.AddJsonBody(pageItem, "application/json");
                var response = client.ExecuteAsync(request, Method.Post).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }
    }
}
