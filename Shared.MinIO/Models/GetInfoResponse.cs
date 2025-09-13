namespace Shared.MinIO.Models;

public class GetInfoResponse
{
    public string? Message { get; set; }
    public bool Result { get; set; }
    public string? FileReference { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string? FileExtension { get; set; }

    public GetInfoResponse(bool result, string? message = null, string? fileExtension = null, string? fileReference = null,
        string? contentType = null, long? fileSize = null)
    {
        Message = message;
        Result = result;
        FileExtension = fileExtension;
        FileReference = fileReference;
        ContentType = contentType;
        FileSize = fileSize;
    }

}