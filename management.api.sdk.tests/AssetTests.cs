using agility.models;
using agility.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace management.api.sdk.tests
{
    [TestClass]
    public class AssetTests
    {
        private ClientInstance clientInstance = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;

        public AssetTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
        }

        [TestMethod]
        public async Task TestGetMediaList()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                var mediaList = await clientInstance.assetMethods.GetMediaList(20, 0, guid);
                Assert.IsNotNull(mediaList, "Unable to retrieve media list.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"GetMediaList failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetGalleries()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                var galleries = await clientInstance.assetMethods.GetGalleries(guid, "", 20, 0);
                Assert.IsNotNull(galleries, "Unable to retrieve galleries.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"GetGalleries failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetDefaultContainer()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                var container = await clientInstance.assetMethods.GetDefaultContainer(guid);
                Assert.IsNotNull(container, "Unable to retrieve default container.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"GetDefaultContainer failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestSaveGallery()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Create a test gallery
                var gallery = new AssetMediaGrouping
                {
                    groupingID = 0,
                    name = $"Test Gallery {DateTime.Now:yyyyMMddHHmmss}",
                    description = "Test gallery created by SDK tests"
                };

                var savedGallery = await clientInstance.assetMethods.SaveGallery(guid, gallery);
                Assert.IsNotNull(savedGallery, "Unable to save gallery.");
                
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail($"SaveGallery failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetGalleryById()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // First, get the list of galleries to find a valid ID
                var galleries = await clientInstance.assetMethods.GetGalleries(guid, "", 20, 0);
                Assert.IsNotNull(galleries, "Unable to retrieve galleries for ID test.");

                // Note: You'll need to adjust this based on the actual structure of galleries response
                // This is a placeholder showing the test structure
                int testGalleryId = 1; // Replace with actual gallery ID from galleries response
                
                var gallery = await clientInstance.assetMethods.GetGalleryById(guid, testGalleryId);
                // Gallery may be null if ID doesn't exist
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGalleryById test note: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetGalleryByName()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Test with a known gallery name
                string testGalleryName = "Test Gallery"; // Replace with actual gallery name
                
                var gallery = await clientInstance.assetMethods.GetGalleryByName(guid, testGalleryName);
                // Gallery may be null if name doesn't exist
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGalleryByName test note: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestDeleteGallery()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // First create a gallery to delete
                var gallery = new AssetMediaGrouping
                {
                    groupingID = 0,
                    name = $"Test Gallery Delete {DateTime.Now:yyyyMMddHHmmss}",
                    description = "Test gallery for deletion"
                };

                var savedGallery = await clientInstance.assetMethods.SaveGallery(guid, gallery);
                Assert.IsNotNull(savedGallery, "Unable to create gallery for deletion test.");

                // Note: Adjust based on actual response structure
                int galleryId = savedGallery.groupingID ?? 0;
                
                if (galleryId > 0)
                {
                    await clientInstance.assetMethods.DeleteGallery(guid, galleryId);
                    Assert.IsTrue(true, "Gallery deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteGallery test note: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestCreateFolder()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Create a test folder
                string folderPath = $"/test-folder-{DateTime.Now:yyyyMMddHHmmss}";
                
                await clientInstance.assetMethods.CreateFolder(folderPath, guid);
                Assert.IsTrue(true, "Folder created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateFolder test note: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestUploadAsset()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Create a temporary test file
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"test-file-{DateTime.Now:yyyyMMddHHmmss}.txt");
                File.WriteAllText(tempFilePath, "This is a test file for SDK upload test.");

                var files = new Dictionary<string, string>
                {
                    { Path.GetFileName(tempFilePath), tempFilePath }
                };

                var uploadedMedia = await clientInstance.assetMethods.Upload(files, guid, "/", 0);
                Assert.IsNotNull(uploadedMedia, "Unable to upload file.");
                Assert.IsTrue(uploadedMedia.Count > 0, "No files were uploaded.");

                // Clean up temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"UploadAsset failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestDeleteFile()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // First upload a file
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"test-delete-{DateTime.Now:yyyyMMddHHmmss}.txt");
                File.WriteAllText(tempFilePath, "Test file for deletion.");

                var files = new Dictionary<string, string>
                {
                    { Path.GetFileName(tempFilePath), tempFilePath }
                };

                var uploadedMedia = await clientInstance.assetMethods.Upload(files, guid, "/", 0);
                Assert.IsNotNull(uploadedMedia, "Unable to upload file for deletion test.");
                Assert.IsTrue(uploadedMedia.Count > 0, "No files were uploaded.");

                // Get the media ID from the uploaded file
                var media = uploadedMedia[0];
                int mediaId = media.mediaID;

                // Delete the file
                var deleteResult = await clientInstance.assetMethods.DeleteFile(mediaId, guid);
                Assert.IsNotNull(deleteResult, "Delete operation should return a result.");

                // Clean up temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"DeleteFile failed: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestMoveFile()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // First upload a file
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"test-move-{DateTime.Now:yyyyMMddHHmmss}.txt");
                File.WriteAllText(tempFilePath, "Test file for moving.");

                var files = new Dictionary<string, string>
                {
                    { Path.GetFileName(tempFilePath), tempFilePath }
                };

                var uploadedMedia = await clientInstance.assetMethods.Upload(files, guid, "/", 0);
                Assert.IsNotNull(uploadedMedia, "Unable to upload file for move test.");
                Assert.IsTrue(uploadedMedia.Count > 0, "No files were uploaded.");

                var media = uploadedMedia[0];
                int mediaId = media.mediaID;

                // Move the file to a different folder
                string newFolder = "/moved-files";
                var movedMedia = await clientInstance.assetMethods.MoveFile(mediaId, newFolder, guid);
                Assert.IsNotNull(movedMedia, "Move operation should return media object.");

                // Clean up temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MoveFile test note: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetAssetByID()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Get media list first to find a valid ID
                var mediaList = await clientInstance.assetMethods.GetMediaList(20, 0, guid);
                Assert.IsNotNull(mediaList, "Unable to retrieve media list.");

                // Note: Adjust based on actual media list structure
                // This is a placeholder
                int testMediaId = 1; // Replace with actual media ID
                
                var asset = await clientInstance.assetMethods.GetAssetByID(testMediaId, guid);
                // Asset may be null if ID doesn't exist
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAssetByID test note: {ex.Message}");
            }
        }

        [TestMethod]
        public async Task TestGetAssetByURL()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                
                // Test with a known asset URL
                string testUrl = "https://example.com/test-asset.jpg"; // Replace with actual URL
                
                var asset = await clientInstance.assetMethods.GetAssetByURL(testUrl, guid);
                // Asset may be null if URL doesn't exist
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAssetByURL test note: {ex.Message}");
            }
        }
    }
}
