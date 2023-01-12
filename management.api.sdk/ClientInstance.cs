using agility.models;
using RestSharp;
using System.Text.Json;

namespace management.api.sdk
{
    public class ClientInstance
    {
        private Options options;
        public ClientInstance(Options _options)
        {
            options = _options;
        }
        private ContentMethods _contentMethods;
        public ContentMethods contentMethods
        {
            get
            {
                if(_contentMethods == null)
                {
                    _contentMethods = new ContentMethods(options); 
                }
                return _contentMethods;
            }
            set { _contentMethods = value; }
        }

        private AssetMethods _assetMethods;
        public AssetMethods assetMethods
        {
            get
            {
                if(_assetMethods == null)
                {
                    _assetMethods = new AssetMethods(options);
                }
                return _assetMethods;
            }
            set { _assetMethods = value; }
        }

        private BatchMethods _batchMethods;
        public BatchMethods batchMethods
        {
            get
            {
                if (_batchMethods == null)
                {
                    _batchMethods = new BatchMethods(options);
                }
                return _batchMethods;
            }
            set { _batchMethods = value; }
        }

        private ContainerMethods _containerMethods;
        public ContainerMethods containerMethods
        {
            get
            {
                if(_containerMethods == null)
                {
                    _containerMethods = new ContainerMethods(options);
                }
                return _containerMethods;
            }
            set { _containerMethods = value; }
        }

        private InstanceUserMethods _instanceUserMethods;
        public InstanceUserMethods instanceUserMethods
        {
            get
            {
                if(_instanceUserMethods == null)
                {
                    _instanceUserMethods = new InstanceUserMethods(options);
                }
                return _instanceUserMethods;
            }
            set { _instanceUserMethods = value; }
        }

        private ModelMethods _modelMethods;
        public ModelMethods modelMethods
        {
            get
            {
                if(_modelMethods == null)
                {
                    _modelMethods = new ModelMethods(options);
                }
                return _modelMethods;
            }
            set { _modelMethods = value; }
        }

        private PageMethods _pageMethods;
        public PageMethods pageMethods
        {
            get
            {
                if(_pageMethods == null)
                {
                    _pageMethods = new PageMethods(options);
                }
                return _pageMethods;
            }
            set { _pageMethods = value; }
        }
    }

    public class ExecuteMethods
    {
        public async Task<RestResponse> ExecuteGet(string apiPath, string guid, string token)
        {
            var baseUrl = DetermineBaseURL(guid);
            var client = new RestClient($"{baseUrl}/api/v1/instance/{guid}");

            client.AddDefaultHeader("Authorization", $"Bearer {token}");
            client.AddDefaultHeader("Cache-Control", "no-cache");

            var request = new RestRequest(apiPath);
            var response = await client.ExecuteAsync(request, Method.Get);

            return response;
        }

        public async Task<RestResponse> ExecutePost(string apiPath, string guid, object? data, string token)
        {
            var baseUrl = DetermineBaseURL(guid);
            var client = new RestClient($"{baseUrl}/api/v1/instance/{guid}");

            client.AddDefaultHeader("Authorization", $"Bearer {token}");
            client.AddDefaultHeader("Cache-Control", "no-cache");

            var request = new RestRequest(apiPath);
            if (data != null)
                request.AddJsonBody(data, "application/json");

            var response = await client.ExecuteAsync(request, Method.Post);

            return response;
        }

        public async Task<RestResponse> ExecutePostFiles(string apiPath, string guid, Dictionary<string, string> files, string token)
        {
            var baseUrl = DetermineBaseURL(guid);
            var client = new RestClient($"{baseUrl}/api/v1/instance/{guid}");

            client.AddDefaultHeader("Authorization", $"Bearer {token}");
            client.AddDefaultHeader("Cache-Control", "no-cache");

            var request = new RestRequest(apiPath, Method.Post) { RequestFormat = DataFormat.Json, AlwaysMultipartFormData = true };

            foreach (var file in files)
            {
                var fileName = file.Key;
                var localPath = file.Value;
                request.AddFile("files", $"{localPath}\\{fileName}");
            }

            var response = await client.ExecuteAsync(request);

            return response;
        }

        public async Task<RestResponse> ExecuteDelete(string apiPath, string guid, string token)
        {
            var baseUrl = DetermineBaseURL(guid);
            var client = new RestClient($"{baseUrl}/api/v1/instance/{guid}");

            client.AddDefaultHeader("Authorization", $"Bearer {token}");
            client.AddDefaultHeader("Cache-Control", "no-cache");

            var request = new RestRequest(apiPath);
            var response = await client.ExecuteAsync(request, Method.Delete);

            return response;
        }

        public string DetermineBaseURL(string guid)
        {
            if (guid.EndsWith("-d"))
            {
                if (Environment.GetEnvironmentVariable("Local") != null)
                {
                    var runLocally = Environment.GetEnvironmentVariable("Local");
                    if (runLocally == "True")
                    {
                        var baseUrl = Environment.GetEnvironmentVariable("BaseLocalURL");
                        return baseUrl;
                    }
                    else
                    {
                        return "https://mgmt-dev.aglty.io";
                    }
                }
                return "https://mgmt-dev.aglty.io";
            }
            else if (guid.EndsWith("-u"))
            {
                return "https://mgmt.aglty.io";
            }
            else if (guid.EndsWith("-c"))
            {
                return "https://mgmt-ca.aglty.io";
            }
            else if (guid.EndsWith("-e"))
            {
                return "https://mgmt-eu.aglty.io";
            }
            else if (guid.EndsWith("-a"))
            {
                return "https://mgmt-aus.aglty.io";
            }
            else
            {
                return "https://mgmt.aglty.io";
            }
        }
    }
}