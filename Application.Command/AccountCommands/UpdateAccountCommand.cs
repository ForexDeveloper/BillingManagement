using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.AccountCommands;

public class UpdateAccountCommand : IRequest
{
    public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IInstallmentRepository _installmentRepository;
        private readonly IBillingRepository _billingRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;

        public UpdateAccountCommandHandler(
            IApplicationDbContextUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IInstallmentRepository installmentRepository,
            IBillingRepository billingRepository,
            IFinancialDocumentRepository financialDocumentRepository)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _installmentRepository = installmentRepository;
            _billingRepository = billingRepository;
            _financialDocumentRepository = financialDocumentRepository;
        }

        public async Task Handle(UpdateAccountCommand command, CancellationToken cancellationToken)
        {
            await UpdateAccounts();
            await UpdateTransactions();
            await UpdateBillings();
            await UpdateInstallments();
            await UpdateFinancialDocuments();
        }

        private async Task UpdateAccounts()
        {
            var accounts = await _accountRepository.GetAllAsync();
            foreach (var account in accounts)
            {
                account.SetCheckSum();
                //account.SetEditDateTime(DateTime.Now);
            }
            _accountRepository.UpdateRange(accounts);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateTransactions()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            foreach (var transaction in transactions)
            {
                transaction.SetCheckSum();
                //transaction.SetEditDateTime(DateTime.Now);
            }
            _transactionRepository.UpdateRange(transactions);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateBillings()
        {
            var billings = await _billingRepository.GetAllAsync();
            foreach (var billing in billings)
            {
                billing.SetCheckSum();
            }
            _billingRepository.UpdateRange(billings);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateInstallments()
        {
            var installments = await _installmentRepository.GetAllAsync();
            foreach (var installment in installments)
            {
                installment.SetCheckSum();
            }
            _installmentRepository.UpdateRange(installments);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task UpdateFinancialDocuments()
        {
            var financialDocuments = await _financialDocumentRepository.GetAllAsync();
            foreach (var financialDocument in financialDocuments)
            {
                financialDocument.SetCheckSum();
                foreach (var financialDocumentPayment in financialDocument.FinancialDocumentPayments)
                {
                    financialDocumentPayment.SetCheckSum();
                }
            }
            _financialDocumentRepository.UpdateRange(financialDocuments);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}