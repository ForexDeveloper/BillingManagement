using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Domain.Core.Entities.CustomerAggregate;
using MediatR;
using Shared.MinIO.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class CalculatePrePaymentAmountQuery : IRequest<decimal>
    {
        public int WalletId { get; set; }
        public decimal Amount { get; }
        public CalculatePrePaymentAmountQuery(int walletId, decimal amount)
        {
            WalletId = walletId;
            Amount = amount;
        }
    }
    public class CalculatePrePaymentAmountQueryHandler : BaseQueryHandler, IRequestHandler<CalculatePrePaymentAmountQuery, decimal>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly IAttachmentReadOnlyRepository _attachmentRepositoryReadOnlyRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;
        private readonly IFileManagerService _fileManagerService;
        private readonly IAttachmentService _attachmentService;
        public CalculatePrePaymentAmountQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepositoryReadOnlyRepository, ICustomerRepository customerRepository, IMerchantReadOnlyRepository merchantReadOnlyRepository, IFileManagerService fileManagerService, IAttachmentService attachmentService)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _attachmentRepositoryReadOnlyRepository = attachmentRepositoryReadOnlyRepository;
            _customerRepository = customerRepository;
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
            _fileManagerService = fileManagerService;
            _attachmentService = attachmentService;
        }

        public async Task<decimal> Handle(CalculatePrePaymentAmountQuery request, CancellationToken cancellationToken)
        {
            var prePaymentAmount = await _walletReadOnlyRepository.CalculatePrePaymentAmount(request.WalletId, request.Amount);

            return prePaymentAmount;
        }

    }
}
