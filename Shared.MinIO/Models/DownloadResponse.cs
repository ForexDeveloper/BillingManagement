namespace Shared.MinIO.Models
{
    public class DownloadResponse
    {
        public byte[]? File { get; set; } = null;
        public string? Message { get; set; } = null;
        public bool Result { get; set; }

        public DownloadResponse(bool result, string? message = null, byte[]? file = null)
        {
            Result = result;
            File = file ?? [];
            Message = message;
        }
    }
}
