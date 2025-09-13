using Application.Service.Dtos.FileManagers;
using Domain.Core.Entities.Document;
using Domain.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shared.MinIO.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.FileManagerCommands
{
    public class UploadDocumentCommand : IRequest<string>
    {
        public IFormFile File { get; set; }
        public AttachmentCategory AttachmentCategory { get; set; }
        public EntityType FolderName { get; set; }

        public UploadDocumentCommand(IFormFile file, EntityType folderName, AttachmentCategory attachmentCategory)
        {
            File = file;
            AttachmentCategory = attachmentCategory;
            FolderName = folderName;
        }
    }

    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, string>
    {
        private readonly IFileManagerService _fileManagerService;
        private readonly UploadFileConfiguration _uploadFileConfiguration;

        public UploadDocumentCommandHandler(
            IFileManagerService fileManagerService,
            IOptions<UploadFileConfiguration> uploadFileConfiguration)
        {
            _fileManagerService = fileManagerService;
            _uploadFileConfiguration = uploadFileConfiguration.Value;
        }

        public async Task<string> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var categoryMappings = GetDocumentConfig();

            if (!categoryMappings.TryGetValue(request.AttachmentCategory, out var config))
            {
                throw new ArgumentOutOfRangeException(nameof(request.AttachmentCategory), request.AttachmentCategory, "نوع دسته بندی فایل پیوست صحیح نمی باشد.");
            }

            var expectedType = config.ExpectedType;
            var expectedMaxFileLength = config.ExpectedMaxFileLength;

            if (string.IsNullOrEmpty(expectedType))
            {
                throw new ArgumentNullException("مقدار expectedType صحیح نمی باشد.");
            }

            if (expectedMaxFileLength == 0)
            {
                throw new ArgumentNullException("مقدار expectedMaxFileLength صحیح نمی باشد.");
            }

            var uploadResponse = await _fileManagerService.Upload(
                request.File,
                request.FolderName.ToString(),
                expectedType,
                expectedMaxFileLength
            );

            if (!uploadResponse.Result)
            {
                throw new DocumentUploadException("خطا در بازگذاری فایل.");
            }

            return uploadResponse.FileReference;
        }

        private Dictionary<AttachmentCategory, (string ExpectedType, long ExpectedMaxFileLength)> GetDocumentConfig()
        {
            var categoryMappings = new Dictionary<AttachmentCategory, (string ExpectedType, long ExpectedMaxFileLength)>
            {
                { AttachmentCategory.CartMeliFront,
                    (
                        _uploadFileConfiguration.TenantMerchantContract.NationalCartImageFront.ExpectedType,
                        _uploadFileConfiguration.TenantMerchantContract.NationalCartImageFront.ExpectedMaxFileLength
                     )
                },
                { AttachmentCategory.CartMeliBack,
                    (
                        _uploadFileConfiguration.TenantMerchantContract.NationalCartImageBack.ExpectedType,
                        _uploadFileConfiguration.TenantMerchantContract.NationalCartImageBack.ExpectedMaxFileLength
                     )
                },
                { AttachmentCategory.OfficialNewspaper,
                    (
                        _uploadFileConfiguration.TenantMerchantContract.OfficialNewspaper.ExpectedType,
                        _uploadFileConfiguration.TenantMerchantContract.OfficialNewspaper.ExpectedMaxFileLength
                     )

                },
                { AttachmentCategory.LeaseAgreement,
                    (
                        _uploadFileConfiguration.TenantMerchantContract.LeaseAgreement.ExpectedType,
                        _uploadFileConfiguration.TenantMerchantContract.LeaseAgreement.ExpectedMaxFileLength
                     )
                },
                { AttachmentCategory.PropertyDeed,
                    (
                        _uploadFileConfiguration.TenantMerchantContract.PropertyDeed.ExpectedType,
                        _uploadFileConfiguration.TenantMerchantContract.PropertyDeed.ExpectedMaxFileLength
                     )
                },
                { AttachmentCategory.BusinessLicense,
                    (
                        _uploadFileConfiguration.TenantMerchantContract.BusinessLicense.ExpectedType,
                        _uploadFileConfiguration.TenantMerchantContract.BusinessLicense.ExpectedMaxFileLength
                     )
                },
                { AttachmentCategory.PlanLogo,
                    (
                        _uploadFileConfiguration.PlanAttachment.ExpectedType,
                        _uploadFileConfiguration.PlanAttachment.ExpectedMaxFileLength
                     )
                }
            };

            return categoryMappings;
        }
    }
}
