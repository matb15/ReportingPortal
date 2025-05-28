using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using Microsoft.Extensions.Options;
using File = Appwrite.Models.File;
using FilePath = System.IO.File;

namespace ReportingPortalServer.Services.AppwriteIO
{
    public interface IAppwriteClient
    {
        Task<string> GetBucketIdAsync(string bucketName);
        Task<string> CreateBucketAsync(string bucketName);
        Task<bool> DeleteBucketAsync(string bucketId);
        Task<string> UploadFileAsync(string bucketId, string filePath, string fileName);
        Task<bool> DeleteFileAsync(string bucketId, string fileId);
    }

    public class AppwriteSettings
    {
        public string Endpoint { get; set; } = default!;
        public string ProjectId { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
    }

    public class AppwriteClient : IAppwriteClient
    {
        private readonly AppwriteSettings _settings;
        private readonly Client _client;
        private readonly Storage _storage;

        public AppwriteClient(IOptions<AppwriteSettings> options)
        {
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options), "Appwrite settings cannot be null.");
            if (string.IsNullOrWhiteSpace(_settings.Endpoint) ||
                string.IsNullOrWhiteSpace(_settings.ProjectId) ||
                string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                throw new ArgumentException("Appwrite settings properties (Endpoint, ProjectId, ApiKey) must not be null or empty.");
            }

            _client = new Client()
                .SetEndpoint(_settings.Endpoint)
                .SetProject(_settings.ProjectId)
                .SetKey(_settings.ApiKey);

            _storage = new Storage(_client);
        }

        public async Task<string> GetBucketIdAsync(string bucketName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            try
            {
                BucketList response = await _storage.ListBuckets();
                foreach (Bucket bucket in response.Buckets)
                {
                    if (bucket.Name == bucketName)
                    {
                        return bucket.Id;
                    }
                }
                throw new Exception($"Bucket with name '{bucketName}' not found.");
            }
            catch (AppwriteException ex)
            {
                throw new Exception("Failed to get bucket ID from Appwrite.", ex);
            }
        }

        public async Task<string> CreateBucketAsync(string bucketName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            }

            try
            {
                Bucket bucket = await _storage.CreateBucket(
                    name: bucketName,
                    bucketId: "unique()",
                    permissions: null,
                    fileSecurity: false,
                    enabled: true,
                    maximumFileSize: null,
                    allowedFileExtensions: null,
                    compression: null,
                    encryption: null,
                    antivirus: null
                );
                return bucket.Id;
            }
            catch (AppwriteException ex)
            {
                throw new Exception("Failed to create bucket in Appwrite.", ex);
            }
        }

        public async Task<bool> DeleteBucketAsync(string bucketId)
        {
            if (string.IsNullOrWhiteSpace(bucketId))
            {
                throw new ArgumentException("Bucket ID cannot be null or empty.", nameof(bucketId));
            }

            try
            {
                await _storage.DeleteBucket(bucketId);
                return true;
            }
            catch (AppwriteException ex)
            {
                throw new Exception($"Failed to delete bucket '{bucketId}' in Appwrite.", ex);
            }
        }

        public async Task<string> UploadFileAsync(string bucketId, string filePath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(bucketId))
            {
                throw new ArgumentException("Bucket ID cannot be null or empty.", nameof(bucketId));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!FilePath.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }

            using var inputFile = FilePath.OpenRead(filePath);

            if (inputFile.Length == 0)
            {
                throw new ArgumentException("The file is empty.", nameof(filePath));
            }

            if (inputFile.Length > 5 * 1024 * 1024) // 5 MB limit
            {
                throw new ArgumentException("The file exceeds the maximum allowed size of 5 MB.", nameof(filePath));
            }

            try
            {
                File file = await _storage.CreateFile(
                    bucketId: bucketId,
                    fileId: "unique()",
                    file: InputFile.FromPath(filePath)
                );
                return file.Id;
            }
            catch (AppwriteException ex)
            {
                throw new Exception("Failed to upload file to Appwrite.", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string bucketId, string fileId)
        {
            if (string.IsNullOrWhiteSpace(bucketId))
            {
                throw new ArgumentException("Bucket ID cannot be null or empty.", nameof(bucketId));
            }

            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new ArgumentException("File ID cannot be null or empty.", nameof(fileId));
            }

            try
            {
                await _storage.DeleteFile(bucketId, fileId);
                return true;
            }
            catch (AppwriteException ex)
            {
                throw new Exception($"Failed to delete file '{fileId}' from bucket '{bucketId}' in Appwrite.", ex);
            }
        }
    }
}