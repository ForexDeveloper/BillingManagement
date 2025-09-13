using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Shared.MinIO.Contracts;
using Shared.MinIO.Enums;
using Shared.MinIO.Models;

namespace Shared.MinIO.Services
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IMinioClient _minioClient;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public FileManagerService(IMinioClient minioClient, IConfiguration configuration)
        {
            _minioClient = minioClient;
            _configuration = configuration;
            _bucketName = _configuration["PublicAppConfiguration:ApplicationName"]?.ToLower() ?? throw new Exception("ApplicationName is not set in app setting");
        }

        public async Task<UploadResponse> Upload(IFormFile file, EntityType folderName, string expectedType, long expectedMaxFileLength)
        {
            try
            {
                if (expectedMaxFileLength < 0 || file.Length > expectedMaxFileLength)
                    return new UploadResponse(false, "File size  is not valid");

                if (string.IsNullOrEmpty(expectedType) || !expectedType.Contains(file.ContentType))
                    return new UploadResponse(false, "Expected type is not valid");

                var exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
                if (!exists)
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));

                var metadata = new Dictionary<string, string>
                {
                    { "extension", $"{Path.GetExtension(file.FileName)}" },
                };

                var prefix = folderName + "/";
                var fileReference = Guid.NewGuid().ToString();
                var stream = file.OpenReadStream();

                var result = await _minioClient.PutObjectAsync(
                    new PutObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(prefix + fileReference)
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType)
                        .WithHeaders(metadata)
                );

                if (string.IsNullOrEmpty(result.Etag))
                    throw new Exception();

                return new UploadResponse(
                    fileReference: fileReference,
                    contentType: file.ContentType,
                    fileExtension: Path.GetExtension(file.FileName),
                    fileSize: file.Length,
                    result: true,
                    message: string.Empty
                    );
            }
            catch (Exception e)
            {
                return new UploadResponse(false, e.Message);
            }
        }
        public async Task<DownloadResponse> Download(string fileReference, EntityType folderName)
        {
            try
            {
                var prefix = folderName + "/";
                fileReference = prefix + fileReference;

                var stream = new MemoryStream();
                var tsc = new TaskCompletionSource<bool>();

                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileReference)
                    .WithCallbackStream(cs =>
                    {
                        cs.CopyTo(stream);
                        tsc.SetResult(true);
                    });

                var result = await _minioClient.GetObjectAsync(getObjectArgs);
                if (string.IsNullOrEmpty(result.ETag))
                    throw new Exception();

                await tsc.Task;
                stream.Seek(0, SeekOrigin.Begin);

                return new DownloadResponse(true, null, stream.ToArray());
            }
            catch (Exception e)
            {
                return new DownloadResponse(false, e.Message);
            }
        }
        public async Task<DeleteResponse> Delete(string fileReference, EntityType folderName)
        {
            try
            {
                var prefix = folderName + "/";
                fileReference = prefix + fileReference;

                await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileReference)
                );

                return new DeleteResponse(true);
            }
            catch (Exception e)
            {
                return new DeleteResponse(false, e.Message);
            }

        }
        public async Task<GetInfoResponse> GetObjectInfo(string fileReference, EntityType folderName)
        {
            try
            {
                var prefix = folderName + "/";
                var fileReferenceWithPrefix = prefix + fileReference;

                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(fileReferenceWithPrefix);

                var statObject = await _minioClient.StatObjectAsync(statObjectArgs);

                if (string.IsNullOrEmpty(statObject.ETag))
                    throw new Exception();

                return new GetInfoResponse(true, null, statObject.MetaData["extension"], fileReference, statObject.ContentType, statObject.Size);
            }
            catch (Exception e)
            {
                return new GetInfoResponse(false, e.Message);
            }
        }
        private static async Task<bool> IsMinioServerRunning(string serverUrl)
        {
            try
            {
                HttpClient _httpClient = new();
                var response = await _httpClient.GetAsync($"http://{serverUrl}/minio/health/live");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception("minio server is not available");
            }
        }
    }
}
