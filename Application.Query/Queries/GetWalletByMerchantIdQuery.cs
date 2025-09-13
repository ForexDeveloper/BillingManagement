using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Wallets;
using Application.Service.Contracts;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using Domain.Core.Entities.MerchantAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using Shared.MinIO.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletByMerchantIdQuery : IRequest<GetWalletByMerchantIdViewModel>
    {
        public int MerchantId { get; set; }
        public int CustomerId { get; }

        public GetWalletByMerchantIdQuery(int customerId, int merchantId)
        {
            CustomerId = customerId;
            MerchantId = merchantId;
        }
    }
    public class GetWalletByTerminalIdQueryHandler : BaseQueryHandler, IRequestHandler<GetWalletByMerchantIdQuery, GetWalletByMerchantIdViewModel>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly IAttachmentReadOnlyRepository _attachmentRepositoryReadOnlyRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly IAttachmentService _attachmentService;
        public GetWalletByTerminalIdQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepositoryReadOnlyRepository, ICustomerRepository customerRepository, IMerchantReadOnlyRepository merchantReadOnlyRepository, IFileManagerService fileManagerService, IAttachmentService attachmentService)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _attachmentRepositoryReadOnlyRepository = attachmentRepositoryReadOnlyRepository;
            _customerRepository = customerRepository;
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
            _fileManagerService = fileManagerService;
            _attachmentService = attachmentService;
        }

        public async Task<GetWalletByMerchantIdViewModel> Handle(GetWalletByMerchantIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _merchantReadOnlyRepository.GetBranchByMerchantIdAsync(request.MerchantId);
            if (branch == null)
                throw new MerchantBranchNotFoundException("شعبه پذیرنده پیدا نشد.");

            var result = await _walletReadOnlyRepository.GetWalletByMerchantIdAsync(request.CustomerId, branch.MerchantId, WalletStatus.Active);

            var attachments = await _attachmentRepositoryReadOnlyRepository.GetListAsync(EntityType.Plan, result.Select(c => c.PlanId).ToList());

            var customer = await _customerRepository.GetAsync(request.CustomerId);
            if (customer == null)
                throw new CustomerNotFoundException("مشتری پیدا نشد.");

            var wallets = new List<GetWalletViewModel>();
            foreach (var wallet in result)
            {
                var planLogo = attachments.FirstOrDefault(a => a.EntityId == wallet.PlanId.ToString());
                var planDetail = wallet.PlanDetails.FirstOrDefault(s => s.Installments.Contains(wallet.NumberOfInstallment));

                wallets.Add(new GetWalletViewModel
                {
                    Balance = wallet.Balance,
                    WalletType = wallet.WalletType,
                    WalletTypeTitle = wallet.WalletType.GetEnumDescription(),
                    WalletStatus = wallet.WalletStatus,
                    WalletStatusTitle = wallet.Balance == 0 ? "بدون اعتبار" : wallet.WalletStatus.GetEnumDescription(),
                    Id = wallet.Id,
                    IsDefault = wallet.IsDefault,
                    Title = wallet.Title,
                    Installment = wallet.NumberOfInstallment,
                    PrepaymentPercent = planDetail?.PrepaymentPercent,
                    PrepaymentMaxAmount = planDetail?.PrepaymentMaxAmount,
                    PrepaymentMinAmount = planDetail?.PrepaymentMinAmount,
                    PlanId = wallet.PlanId,
                    PlanLogo = planLogo != null ? await _attachmentService.GetDocumentObject(EntityType.Plan.ToString(), planLogo.FileReference, planLogo.ContentType) : null,
                });
            }

            var walletWithCustomer = new GetWalletByMerchantIdViewModel
            {
                CustomerId = request.CustomerId,
                CustomerFullName = customer.FullName,
                MerchantBranchTitle = branch?.Title,
                Wallets = wallets,
            };

            return walletWithCustomer;
        }

    }
}
