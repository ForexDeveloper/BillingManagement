using Application.Service.Contracts;
using Application.Service.Dtos.Attachments;
using Application.Service.Dtos.FileManagers;
using Domain.Core.Entities.Document;
using Shared.IdentityServerProvider.Contracts;
using Shared.MinIO.Contracts;
using Shared.MinIO.Entities;
using System.Threading.Tasks;

namespace Application.Service.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IFileManagerService _fileManagerService;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly ICurrentUserService _currentUserService;

        public AttachmentService(IFileManagerService fileManagerService,
            IAttachmentRepository attachmentRepository, ICurrentUserService currentUserService)
        {
            _fileManagerService = fileManagerService;
            _attachmentRepository = attachmentRepository;
            _currentUserService = currentUserService;
        }

        public async Task UpdateAsync(AttachmentDto attachment)
        {
            var existAttachment = await _attachmentRepository.GetAttachmentAsync(attachment.EntityId, (byte)attachment.EntityType, (byte)attachment.AttachmentCategory);
            if (existAttachment == null)
            {
                var objectInfo = await _fileManagerService.GetObjectInfo(attachment.FileReference, attachment.EntityType.ToString());

                if (objectInfo == null || !objectInfo.Result)
                    throw new DocumentNotFoundException("فایل یافت نشد.");

                var attas = new Attachment(
                    (byte)attachment.EntityType,
                    objectInfo.FileReference,
                    attachment.EntityId.ToString(),
                    objectInfo.FileSize.GetValueOrDefault(),
                    objectInfo.FileExtension,
                    objectInfo.ContentType,
                    (byte)attachment.AttachmentCategory,
                    _currentUserService.UserId,
                    _currentUserService.ClientId
                );
                await _attachmentRepository.AddAsync(attas);
            }
        }
        public async Task CreateAsync(AttachmentDto attachment)
        {
            var objectInfo = await _fileManagerService.GetObjectInfo(attachment.FileReference, attachment.EntityType.ToString());

            if (objectInfo == null || !objectInfo.Result)
                throw new DocumentNotFoundException("فایل یافت نشد.");

            var attas = new Attachment(
                (byte)attachment.EntityType,
                objectInfo.FileReference,
                attachment.EntityId.ToString(),
                objectInfo.FileSize.GetValueOrDefault(),
                objectInfo.FileExtension,
                objectInfo.ContentType,
                (byte)attachment.AttachmentCategory,
                _currentUserService.UserId,
                _currentUserService.ClientId
            );
            await _attachmentRepository.AddAsync(attas);

        }

        public async Task<DownloadDocumentsVm> GetDocumentObject(string folderName, string fileReference, string contentType)
        {
            DownloadDocumentsVm objectDocument = null;

            if (!string.IsNullOrEmpty(fileReference))
            {
                var fileReferenceResponse = await _fileManagerService.Download(fileReference, folderName);
                objectDocument = new DownloadDocumentsVm(fileReferenceResponse.File, contentType, fileReference);
            }

            return objectDocument;
        }
    }
}
