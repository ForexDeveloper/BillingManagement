namespace Shared.MinIO.Models
{
    public class UploadResponse
    {
        public string? FileReference { get; set; } = null;
        public long? FileSize { get; set; } = null;
        public string? FileExtension { get; set; } = null;
        public string? ContentType { get; set; } = null;
        public string? Message { get; set; } = null;
        public bool Result { get; set; }

        public UploadResponse(bool result, string? message = null, string? fileReference = null, string? contentType = null, string? fileExtension = null, long? fileSize = null)
        {
            FileReference = fileReference;
            ContentType = contentType;
            FileExtension = fileExtension;
            FileSize = fileSize;
            Result = result;
            Message = message;
        }
    }

}
