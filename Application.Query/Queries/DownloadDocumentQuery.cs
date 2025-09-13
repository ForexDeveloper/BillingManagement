using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Attachments;
using Domain.Core.Entities.Document;
using Domain.Core.Enums;
using MediatR;
using Shared.MinIO.Attributes;
using Shared.MinIO.Contracts;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class DownloadDocumentQuery : IRequest<DownloadDocumentVm>
{
    public Guid FileReference { get; }
    public DownloadDocumentQuery(Guid fileReference)
    {
        FileReference = fileReference;
    }
}

public class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, DownloadDocumentVm>
{
    private readonly IFileManagerService _fileManagerService;
    private readonly IAttachmentReadOnlyRepository _attachmentReadOnlyRepository;

    public DownloadDocumentQueryHandler(IFileManagerService fileManagerService, IAttachmentReadOnlyRepository attachmentReadOnlyRepository)
    {
        _fileManagerService = fileManagerService;
        _attachmentReadOnlyRepository = attachmentReadOnlyRepository;
    }

    public async Task<DownloadDocumentVm> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _attachmentReadOnlyRepository.GetByFileReference(request.FileReference);

        if (attachment == null || string.IsNullOrEmpty(attachment.ContentType))
        {
            throw new DocumentNotFoundException("فایل پیدا نشد.");
        }

        var category = attachment.AttachmentCategory;
        var memberInfo = category.GetType().GetMember(category.ToString()).FirstOrDefault();
        var isSensitive = memberInfo?.GetCustomAttribute<SensitiveAttribute>() != null;
        if (isSensitive)
        {
            throw new DocumentNotFoundException("فایل پیدا نشد.");
        }

        var downloadResponse = await _fileManagerService.Download(request.FileReference.ToString(), ((EntityType)attachment.EntityType).ToString());
        if (!downloadResponse.Result || downloadResponse.File == null)
        {
            throw new DocumentNotFoundException("فایل پیدا نشد.");
        }

        return new DownloadDocumentVm(downloadResponse.File, attachment.ContentType);
    }

}
