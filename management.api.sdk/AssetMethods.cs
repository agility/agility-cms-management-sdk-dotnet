using agility.models;
using RestSharp;
using System.Text;

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

        public async Task<string?> Upload(Dictionary<string, string> files, string agilityFolderPath, bool overwrite = false, int groupingID = -1)
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
                var response = client.ExecuteAsync(request, Method.Post).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> DeleteFile(int? mediaID)
        {
            try
            {
                var request = new RestRequest($"/asset/delete/{mediaID}");
                var response = client.ExecuteAsync(request, Method.Delete).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
           
        }

        public async Task<string?> MoveFile(int? mediaID, string? newFolder)
        {
            try
            {
                var request = new RestRequest($"/asset/move/{mediaID}?newFolder={newFolder}");
                var response = client.ExecuteAsync(request, Method.Post).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetMediaList(int? pageSize, int? recordOffset)
        {
            try
            {
                var request = new RestRequest($"/asset/list?pageSize={pageSize}&recordOffset={recordOffset}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetAssetByID(int? mediaID)
        {
            try
            {
                var request = new RestRequest($"/asset/{mediaID}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetAssetByURL(string? url)
        {
            try
            {
                var request = new RestRequest($"/asset?url={url}");
                var response = client.ExecuteAsync(request, Method.Get).Result.Content;
                return response;
            }
            catch
            {
                throw;
            }
        }
    }
}
