using Application.Service.Dtos.Attachments;
using Application.Service.Dtos.FileManagers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Service.Contracts
{
    public interface IAttachmentService
    {
        Task CreateAsync(AttachmentDto attachment);
        Task UpdateAsync(AttachmentDto attachment);
        Task<DownloadDocumentsVm> GetDocumentObject(string folderName, string fileReference, string contentType);
    }
}
