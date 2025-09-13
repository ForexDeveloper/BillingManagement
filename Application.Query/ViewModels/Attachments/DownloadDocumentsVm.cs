namespace Application.Query.ViewModels.Attachments;

public class DownloadDocumentsVm
{
    public byte[]? File { get; set; }
    public string ContentType { get; set; }
    public string FileReference { get; set; }

    public DownloadDocumentsVm(byte[]? file, string contentType, string fileReference)
    {
        File = file;
        ContentType = contentType;
        FileReference = fileReference;
    }
}