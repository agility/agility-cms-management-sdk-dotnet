using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class AssetMethods
    {
        private Options _options;
        ExecuteMethods executeMethods;
        public AssetMethods(Options options)
        {
            _options = options;
            executeMethods = new ExecuteMethods();
        }

        /// <summary>
        /// Method to upload multiple multiple files.
        /// </summary>
        /// <param name="files">List of files with Key as file name and Value as the local path.</param>
        /// <param name="guid">Current website guid.</param>
        /// <param name="agilityFolderPath">Path of the folder in Agility where the file(s) needs to be uploaded.</param>
        /// <param name="groupingID">The groupingID to which the file belongs.</param>
        /// <returns>A collection of Media class Object.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<List<Media>?> Upload(Dictionary<string, string> files, string guid, string agilityFolderPath, int groupingID = -1)
        {
            try
            {
                var apiPath = $"/asset/upload?folderPath={agilityFolderPath}&groupingID={groupingID}";

                var response = executeMethods.ExecutePostFiles(apiPath, guid, files, _options.token);

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

        public async Task<Media?> CreateFolder(string originKey, string guid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originKey))
                {
                    throw new ApplicationException("Please provide the originKey to upload the folder.");
                }
                var apiPath = $"/asset/folder?originKey={originKey}";

                var response = executeMethods.ExecutePost(apiPath, guid, null, _options.token);

                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new ApplicationException($"Unable to create folder. Additional Details: {response.Result.Content}");
                }

                var options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                var media = JsonSerializer.Deserialize<Media?>(response.Result.Content, options);
                return media;
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
        /// <param name="guid">Current website guid.</param>
        /// <returns>Returns a string response if a file has been deleted.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string?> DeleteFile(int? mediaID, string guid)
        {
            try
            {
                var apiPath = $"/asset/delete/{mediaID}";
                var response = executeMethods.ExecuteDelete(apiPath, guid, _options.token);
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
        /// <param name="guid">Current website guid.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Media?> MoveFile(int? mediaID, string? newFolder, string guid)
        {
            try
            {
                var apiPath = $"/asset/move/{mediaID}?newFolder={newFolder}";
                var response = executeMethods.ExecutePost(apiPath, guid, null, _options.token);
 
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
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of AssetMediaList class.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<AssetMediaList?> GetMediaList(int? pageSize, int? recordOffset, string guid)
        {
            try
            {
                var apiPath = $"/asset/list?pageSize={pageSize}&recordOffset={recordOffset}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
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
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of Media class with the information of the asset.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Media?> GetAssetByID(int? mediaID, string guid)
        {
            try
            {
                var apiPath = $"/asset/{mediaID}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);
 
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
        /// <param name="guid">Current website guid.</param>
        /// <returns>An object of Media class with the information of the asset.</returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<Media?> GetAssetByURL(string? url, string guid)
        {
            try
            {
                var apiPath = $"/asset?url={url}";
                var response = executeMethods.ExecuteGet(apiPath, guid, _options.token);

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
