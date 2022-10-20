using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class AssetMethods
    {
        ClientInstance _clientInstance = null;
        public readonly RestClient client = null;
        private Options _options = null;

        public AssetMethods(Options options)
        {
            _options = options;
            _clientInstance = new ClientInstance();
            client = _clientInstance.CreateClient(_options);
        }

        public async Task<List<Media>?> Upload(Dictionary<string, string> files, string agilityFolderPath, bool overwrite = false, int groupingID = -1)
        {
            try
            {
                var request = new RestRequest($"/asset/upload?folderPath={agilityFolderPath}&overwrite={overwrite}&groupingID={groupingID}");
                foreach (var file in files)
                {
                    var fileName = file.Key;
                    var localPath = file.Value;
                    request.AddFile(fileName, $"{localPath}\\{fileName}");
                }
                var response = client.ExecuteAsync(request, Method.Post);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to upload the media. Additional Details: {response.Result.Content}");
                }
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var medias = JsonSerializer.Deserialize<List<Media>>(response.Result.Content, options);
                return medias;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string?> DeleteFile(int? mediaID)
        {
            try
            {
                var request = new RestRequest($"/asset/delete/{mediaID}");
                var response = client.ExecuteAsync(request, Method.Delete);
                if(response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to delete the media for mediaID: {mediaID}. Additional Details: {response.Result.Content}");
                }
                return response.Result.Content;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<Media?> MoveFile(int? mediaID, string? newFolder)
        {
            try
            {
                var request = new RestRequest($"/asset/move/{mediaID}?newFolder={newFolder}");
                var response = client.ExecuteAsync(request, Method.Post);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to move the file for mediaID: {mediaID}. Additional Details: {response.Result.Content}");
                }
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var media = JsonSerializer.Deserialize<Media>(response.Result.Content, options);
                return media;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AssetMediaList?> GetMediaList(int? pageSize, int? recordOffset)
        {
            try
            {
                var request = new RestRequest($"/asset/list?pageSize={pageSize}&recordOffset={recordOffset}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve assets for the website. Additional Details: {response.Result.Content}");
                }
                
                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var mediaList = JsonSerializer.Deserialize<AssetMediaList>(response.Result.Content, options);
                return mediaList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Media?> GetAssetByID(int? mediaID)
        {
            try
            {
                var request = new RestRequest($"/asset/{mediaID}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve asset for mediaID {mediaID}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var media = JsonSerializer.Deserialize<Media>(response.Result.Content, options);
                return media;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Media?> GetAssetByURL(string? url)
        {
            try
            {
                var request = new RestRequest($"/asset?url={url}");
                var response = client.ExecuteAsync(request, Method.Get);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to retrieve asset for url {url}. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var media = JsonSerializer.Deserialize<Media>(response.Result.Content, options);
                return media;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
