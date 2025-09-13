using Microsoft.AspNetCore.Http;
using Shared.MinIO.Enums;
using Shared.MinIO.Models;

namespace Shared.MinIO.Contracts
{
    public interface IFileManagerService
    {
        Task<UploadResponse> Upload(IFormFile file, EntityType folderName, string expectedType, long expectedMaxFileLength);
        Task<DownloadResponse> Download(string fileReference, EntityType folderName);
        Task<DeleteResponse> Delete(string fileReference, EntityType folderName);
        Task<GetInfoResponse> GetObjectInfo(string fileReference, EntityType folderName);
    }
}
