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

        /// <summary>
        /// Method to upload multiple multiple files.
        /// </summary>
        /// <param name="files">List of files with Key as file name and Value as the local path.</param>
        /// <param name="agilityFolderPath">Path of the folder in Agility where the file(s) needs to be uploaded.</param>
        /// <param name="groupingID">The groupingID to which the file belongs.</param>
        /// <returns>A collection of Media class Object.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Media>?> Upload(Dictionary<string, string> files, string agilityFolderPath, int groupingID = -1)
        {
            try
            {
                var request = new RestRequest($"/asset/upload?folderPath={agilityFolderPath}&groupingID={groupingID}", Method.Post) { RequestFormat = DataFormat.Json, AlwaysMultipartFormData = true };

                foreach (var file in files)
                {
                    var fileName = file.Key;
                    var localPath = file.Value;
                    request.AddFile("files", $"{localPath}\\{fileName}");
                }
                var response = client.ExecuteAsync(request);

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

        /// <summary>
        /// Method to delete a file on the basis of a valid mediaID.
        /// </summary>
        /// <param name="mediaID">The mediaID of the asset which needs to be deleted.</param>
        /// <returns>Returns a string response if a file has been deleted.</returns>
        /// <exception cref="ApplicationException"></exception>
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

        /// <summary>
        /// Method to move an existing file from one folder.
        /// </summary>
        /// <param name="mediaID">The mediaID of the file that needs to be moved.</param>
        /// <param name="newFolder">The new location where the file needs to be moved.</param>
        /// <returns>An object of Media class with the new location of the file.</returns>
        /// <exception cref="ApplicationException"></exception>
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

        /// <summary>
        /// Method to get the list of assets for the website.
        /// </summary>
        /// <param name="pageSize">The pageSize on which the assets needs to selected.</param>
        /// <param name="recordOffset">The record offset value to skip search results.</param>
        /// <returns>An object of AssetMediaList class.</returns>
        /// <exception cref="ApplicationException"></exception>
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

        /// <summary>
        /// Method to get the information of an Asset on the basis of the mediaID.
        /// </summary>
        /// <param name="mediaID">The mediaID of the requested asset.</param>
        /// <returns>An object of Media class with the information of the asset.</returns>
        /// <exception cref="ApplicationException"></exception>

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

        /// <summary>
        /// Method to get the information of an Asset on the basis of the url.
        /// </summary>
        /// <param name="url">The url  of the requested asset.</param>
        /// <returns>An object of Media class with the information of the asset.</returns>
        /// <exception cref="ApplicationException"></exception>
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
