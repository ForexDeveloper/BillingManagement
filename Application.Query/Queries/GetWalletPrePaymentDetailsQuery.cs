using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Application.Service.Dtos.Wallet;
using Domain.Core.Entities.CustomerAggregate;
using MediatR;
using Shared.MinIO.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletPrePaymentDetailsQuery : IRequest<GetWalletPrePaymentDetails>
    {
        public int WalletId { get; set; }
        public decimal Amount { get; }
        public GetWalletPrePaymentDetailsQuery(int walletId, decimal amount)
        {
            WalletId = walletId;
            Amount = amount;
        }
    }
    public class GetWalletPrePaymentDetailsQueryHandler : BaseQueryHandler, IRequestHandler<GetWalletPrePaymentDetailsQuery, GetWalletPrePaymentDetails>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly IAttachmentReadOnlyRepository _attachmentRepositoryReadOnlyRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly IAttachmentService _attachmentService;
        public GetWalletPrePaymentDetailsQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepositoryReadOnlyRepository, ICustomerRepository customerRepository, IMerchantReadOnlyRepository merchantReadOnlyRepository, IFileManagerService fileManagerService, IAttachmentService attachmentService)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _attachmentRepositoryReadOnlyRepository = attachmentRepositoryReadOnlyRepository;
            _customerRepository = customerRepository;
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
            _fileManagerService = fileManagerService;
            _attachmentService = attachmentService;
        }

        public async Task<GetWalletPrePaymentDetails> Handle(GetWalletPrePaymentDetailsQuery request, CancellationToken cancellationToken)
        {
            var walletPrePaymentDetails = await _walletReadOnlyRepository.GetWalletPrePaymentDetails(request.WalletId, request.Amount);

            return walletPrePaymentDetails;
        }

    }
}
