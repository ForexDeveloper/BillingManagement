namespace Application.Query.ViewModels.Attachments
{
    public class DownloadDocumentVm
    {
        public byte[] File { get; set; }
        public string ContentType { get; set; }

        public DownloadDocumentVm(byte[] file, string contentType)
        {
            File = file;
            ContentType = contentType;
        }
    }
}
