using Application.Command.TransactionCommands.Dtos;
using Application.Service.Contracts;
using Application.Service.Dtos.FinancialDocuments;
using Application.Service.Dtos.Transactions;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.BillingAggregate.Exceptions;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.CustomerAggregate.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Constants;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Command.BillingCommands;

public class CreateBillingPaymentCommand : IRequest<List<SetTransactionDto>>
{
    public long BillingId { get; set; }
    public long PaymentId { get; set; }
    public int TenantId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public List<PaymentDetailDto> PaymentDetails { get; set; }

    public CreateBillingPaymentCommand(int tenantId, int customerId, decimal amount, long billingId,
        long paymentId, List<PaymentDetailDto> paymentDetails)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Amount = amount;
        PaymentId = paymentId;
        BillingId = billingId;
        PaymentDetails = paymentDetails;
    }

    public class CreateBillingPaymentCommandHandler : IRequestHandler<CreateBillingPaymentCommand, List<SetTransactionDto>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IBillingRepository _billingRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IFinancialDocumentRepository _financialDocumentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IInstallmentRepository _installmentRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ITransactionService _transactionService;
        private readonly IWalletRepository _walletRepository;
        private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;

        public CreateBillingPaymentCommandHandler(
            ITenantRepository tenantRepository,
            IFinancialDocumentRepository financialDocumentRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ICustomerRepository customerRepository,
            IBillingRepository billingRepository,
            IInstallmentRepository installmentRepository,
            ITransactionService transactionService,
            IWalletRepository walletRepository,
            ITenantPlatformContractRepository tenantPlatformContractRepository)
        {
            _tenantRepository = tenantRepository;
            _financialDocumentRepository = financialDocumentRepository;
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _customerRepository = customerRepository;
            _billingRepository = billingRepository;
            _installmentRepository = installmentRepository;
            _transactionService = transactionService;
            _walletRepository = walletRepository;
            _tenantPlatformContractRepository = tenantPlatformContractRepository;
        }

        public async Task<List<SetTransactionDto>> Handle(CreateBillingPaymentCommand request, CancellationToken cancellationToken)
        {
            var existingFinancialDocument = await _financialDocumentRepository.GetAsync(request.PaymentId);
            if (existingFinancialDocument != null)
            {
                return existingFinancialDocument.FinancialDocumentPayments.Select(x => new SetTransactionDto
                {
                    PaymentDetailId = x.PaymentDetailId.Value,
                    TransactionId = x.TransactionId
                }).ToList();
            }

            var paymentDetail = request.PaymentDetails.FirstOrDefault() ??
                throw new ArgumentValidationException(nameof(request.PaymentDetails), "جزییات پرداخت الزامی است.");

            var tenant = await _tenantRepository.GetAsync(request.TenantId) ?? throw new TenantNotFoundException("مالک زیر ساخت یافت نشد");
            var currentBilling = await ValidateAndGetBilling(request);
            await SetCashWallet(request);

            List<FinancialDocumentPayment> financialDocumentPayments = [];

            List<SetTransactionDto> setTransactionDtos = [];

            decimal installmentAmountTransaction = 0;
            decimal interestAmountTransaction = 0;
            decimal penaltiesAmountTransaction = 0;

            //calculate penalty and update bill on get bill customer method
            var hasNotCalculatedPenalty = await _billingRepository.HasNotCalculatedPenalty(currentBilling.Id);

            if (hasNotCalculatedPenalty)
                throw new BillingPaymentException("در حال حاضر امکان پرداخت صورت حساب وجود ندارد.");

            var parentInstallmentIds = await _billingRepository.GetInstallmentIdsAsync(currentBilling.Id);
            var installments = await _installmentRepository.GetByIds(parentInstallmentIds);

            var billingRemainAmount = currentBilling.Amount
                + currentBilling.PreviousDebitAmount
                + currentBilling.PreviousPenaltyAmount
                - currentBilling.PreviousCreditAmount
                - currentBilling.BillingPayments.Sum(x => x.Amount);

            if (request.Amount > billingRemainAmount)
                throw new ArgumentValidationException(nameof(request.Amount), ".مبلغ پرداختی بیش از باقیمانده صورتحساب می باشد");

            var currentPayment = request.Amount;

            var businessIdentityIds = new List<int>() { request.CustomerId, request.TenantId };

            var accounts = await _accountRepository.GetByBusinessIdentityIds(businessIdentityIds, request.TenantId);

            var tenantBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.TenantId && x.Type == AccountType.Bank);
            var tenantLoanAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.TenantId && x.Type == AccountType.Loan);
            var tenantInterestAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.TenantId && x.Type == AccountType.Interest);
            var tenantPenaltyAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.TenantId && x.Type == AccountType.Penalty);

            var customerBankAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.CustomerId && x.Type == AccountType.Bank);
            var customerCashwalletAccount = accounts.FirstOrDefault(x => x.BusinessIdentityId == request.CustomerId && x.Type == AccountType.CashWallet);


            using (TransactionScope transactionScope = new(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (billingRemainAmount == currentPayment)
                {
                    var installmentNotCompletePaidAmount = 0m;
                    var interestNotCompletePaidAmount = 0m;
                    var penaltiesNotCompletePaidAmount = 0m;

                    foreach (var installment in installments)
                    {
                        if (installment.State == InstallmentState.CompletePaid) continue;

                        switch (installment.Type)
                        {
                            case InstallmentType.Installment:
                                installmentNotCompletePaidAmount += (installment.Amount - installment.PaidAmount);
                                break;

                            case InstallmentType.Interest:
                                interestNotCompletePaidAmount += (installment.Amount - installment.PaidAmount);
                                break;

                            case InstallmentType.Penalty:
                                penaltiesNotCompletePaidAmount += (installment.Amount - installment.PaidAmount);
                                break;
                        }

                        installment.UpdateStatus(InstallmentState.CompletePaid);
                        installment.SetPaidAmount(installment.Amount);
                    }

                    installmentAmountTransaction += installmentNotCompletePaidAmount;
                    interestAmountTransaction += interestNotCompletePaidAmount;
                    penaltiesAmountTransaction += penaltiesNotCompletePaidAmount;
                }
                else
                {
                    var penaltiesInstallments = installments
                        .Where(x => x.Type == InstallmentType.Penalty && x.State != InstallmentState.CompletePaid)
                        .OrderBy(x => x.Id)
                        .ToList();

                    var interestInstallments = installments
                        .Where(x => x.Type == InstallmentType.Interest && x.State != InstallmentState.CompletePaid)
                        .OrderBy(x => x.Id)
                        .ToList();

                    var parentInstallments = installments
                        .Where(x => x.Type == InstallmentType.Installment && x.State != InstallmentState.CompletePaid)
                        .OrderBy(x => x.Id)
                        .ToList();

                    foreach (var penaltyInstallment in penaltiesInstallments)
                    {
                        if (currentPayment <= 0)
                            break;

                        var remainPenaltyAmount = penaltyInstallment.Amount - penaltyInstallment.PaidAmount;

                        if (currentPayment >= remainPenaltyAmount)
                        {
                            currentPayment -= remainPenaltyAmount;
                            penaltyInstallment.UpdateStatus(InstallmentState.CompletePaid);
                            penaltyInstallment.SetPaidAmount(penaltyInstallment.Amount);
                            penaltiesAmountTransaction += remainPenaltyAmount;
                        }
                        else
                        {
                            penaltyInstallment.UpdateStatus(InstallmentState.PartiallyPaid);
                            penaltyInstallment.UpdatePaidAmount(currentPayment);
                            penaltiesAmountTransaction += currentPayment;
                            currentPayment = 0;
                        }
                    }

                    foreach (var interestInstallment in interestInstallments)
                    {
                        if (currentPayment <= 0)
                            break;

                        var remainInterestAmount = interestInstallment.Amount - interestInstallment.PaidAmount;

                        if (currentPayment >= remainInterestAmount)
                        {
                            currentPayment -= remainInterestAmount;
                            interestInstallment.UpdateStatus(InstallmentState.CompletePaid);
                            interestInstallment.SetPaidAmount(interestInstallment.Amount);
                            interestAmountTransaction += remainInterestAmount;
                        }
                        else
                        {
                            interestInstallment.UpdateStatus(InstallmentState.PartiallyPaid);
                            interestInstallment.UpdatePaidAmount(currentPayment);
                            interestAmountTransaction += currentPayment;
                            currentPayment = 0;
                        }
                    }

                    foreach (var parentInstallment in parentInstallments)
                    {
                        if (currentPayment <= 0)
                            break;

                        var remainInstallmentAmount = parentInstallment.Amount - parentInstallment.PaidAmount;

                        if (currentPayment >= remainInstallmentAmount)
                        {
                            currentPayment -= remainInstallmentAmount;
                            parentInstallment.UpdateStatus(InstallmentState.CompletePaid);
                            parentInstallment.SetPaidAmount(parentInstallment.Amount);
                            installmentAmountTransaction += remainInstallmentAmount;
                        }
                        else
                        {
                            parentInstallment.UpdateStatus(InstallmentState.PartiallyPaid);
                            parentInstallment.UpdatePaidAmount(currentPayment);
                            installmentAmountTransaction += currentPayment;
                            currentPayment = 0;
                        }
                    }
                }

                var newChildTransactions = new List<TransactionDto>();

                if (installmentAmountTransaction > 0)
                {
                    var newChildTransaction = new TransactionDto()
                    {
                        FromAccountId = customerCashwalletAccount.Id,
                        ToAccountId = tenantLoanAccount.Id,
                        TenantId = request.TenantId,
                        Amount = installmentAmountTransaction,
                        TransactionType = TransactionType.Charge,
                        Description = string.Format(TransactionDescriptionConstants.CHARGE_TO_TENANT_LOAN, tenant.Title)
                    };

                    newChildTransactions.Add(newChildTransaction);
                }

                if (interestAmountTransaction > 0)
                {
                    var newChildTransaction = new TransactionDto()
                    {
                        FromAccountId = customerCashwalletAccount.Id,
                        ToAccountId = tenantInterestAccount.Id,
                        TenantId = request.TenantId,
                        Amount = interestAmountTransaction,
                        TransactionType = TransactionType.Charge,
                        Description = string.Format(TransactionDescriptionConstants.CHARGE_TO_TENANT_INTEREST, tenant.Title)
                    };

                    newChildTransactions.Add(newChildTransaction);
                }

                if (penaltiesAmountTransaction > 0)
                {
                    var newChildTransaction = new TransactionDto()
                    {
                        FromAccountId = customerCashwalletAccount.Id,
                        ToAccountId = tenantPenaltyAccount.Id,
                        TenantId = request.TenantId,
                        Amount = penaltiesAmountTransaction,
                        TransactionType = TransactionType.Charge,
                        Description = string.Format(TransactionDescriptionConstants.CHARGE_TO_TENANT_PENALTY, tenant.Title)
                    };

                    newChildTransactions.Add(newChildTransaction);
                }

                var newTransactions = new List<TransactionDto>()
                {
                    new()
                    {
                        FromAccountId = customerBankAccount.Id,
                        ToAccountId = tenantBankAccount.Id,
                        TenantId = request.TenantId,
                        Amount = paymentDetail.Amount,
                        TransactionType = TransactionType.Transfer,
                        Description = string.Format(TransactionDescriptionConstants.PAYMENT_TO_TENANT_BANK, tenant.Title),
                        Children =
                            [
                                new()
                                {
                                    FromAccountId = tenantBankAccount.Id,
                                    ToAccountId = customerCashwalletAccount.Id,
                                    TenantId = request.TenantId,
                                    TransactionType = TransactionType.Charge,
                                    Description = TransactionDescriptionConstants.CHARGE_CASH_WALLET,
                                    Amount = paymentDetail.Amount,
                                    Children = newChildTransactions
                                }
                            ]
                    }
                };

                try
                {
                    var transactions = await _transactionService.CreateTransactionAsync(newTransactions, transactionScope);

                    var customerPaymentTransaction = transactions.FirstOrDefault(x => x.FromAccountId == customerBankAccount.Id &&
                        x.ToAccountId == tenantBankAccount.Id && x.Type == TransactionType.Transfer && x.ParentId == null);

                    var tenantChargeTransaction = transactions.FirstOrDefault(x => x.FromAccountId == tenantBankAccount.Id &&
                        x.ToAccountId == customerCashwalletAccount.Id && x.Type == TransactionType.Charge
                        && x.ParentId == customerPaymentTransaction.Id);

                    if (billingRemainAmount == request.Amount)
                    {
                        var newBillingPayment = new BillingPayment(currentBilling.Id, DateTime.Now, billingRemainAmount,
                            customerPaymentTransaction, BillingPaymentState.Paid, paymentDetail.PaymentDetailId);

                        currentBilling.UpdateState(BillingState.CompletePaid);
                        currentBilling.AddBillingPayment(newBillingPayment);
                    }
                    else
                    {
                        var newBillingPayment = new BillingPayment(currentBilling.Id, DateTime.Now, request.Amount,
                            customerPaymentTransaction, BillingPaymentState.Paid, paymentDetail.PaymentDetailId);

                        currentBilling.UpdateState(BillingState.PartiallyPaid);
                        currentBilling.AddBillingPayment(newBillingPayment);
                    }

                    financialDocumentPayments.Add(new FinancialDocumentPayment(
                        request.CustomerId,
                        currentBilling.ToBusinessIdentityId,
                        FinancialDocumentPaymentType.Cash,
                        request.Amount,
                        paymentDetail.WalletId,
                        paymentDetail.WalletContractId,
                        customerPaymentTransaction,
                        paymentDetail.PaymentDetailId));


                    var financialDocument = await CreateFinancialDocument(financialDocumentPayments, currentBilling.ToBusinessIdentityId, request);

                    await _financialDocumentRepository.AddAsync(financialDocument);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    transactionScope.Complete();

                    setTransactionDtos.Add(new SetTransactionDto
                    {
                        PaymentDetailId = paymentDetail.PaymentDetailId,
                        TransactionId = customerPaymentTransaction.Id
                    });

                    return setTransactionDtos.GroupBy(x => x.PaymentDetailId).Select(x => x.First()).ToList();
                }
                catch (Exception)
                {
                    transactionScope.Dispose();
                    throw;
                }
            }
        }

        private async Task<Billing> ValidateAndGetBilling(CreateBillingPaymentCommand command)
        {
            var customer = await _customerRepository.GetAsync(command.CustomerId)
                ?? throw new CustomerNotFoundException("مشتری یافت نشد.");

            if (customer.TenantId != command.TenantId)
            {
                throw new ArgumentValidationException(nameof(command.CustomerId), "این مشتری به این مالک زیرساخت تعلق ندارد");
            }

            var billing = await _billingRepository.GetByIdAsync(command.BillingId);
            if (billing == null)
            {
                throw new BillingNotFoundException("صورت حساب یافت نشد.");
            }

            if (billing.State == BillingState.CompletePaid)
            {
                throw new ArgumentValidationException(nameof(command.BillingId), "صورت حساب قبلا پرداخت شده است.");
            }

            var isExistPreviousNotCompletePaidBillingAsync = await _billingRepository.IsExistPreviousNotCompletePaidBillingAsync(billing.FromAccountId, command.BillingId);
            if (isExistPreviousNotCompletePaidBillingAsync)
            {
                throw new ArgumentValidationException(nameof(command.BillingId), "مشکلی رخ داده است لطفا دقایقی دیگر مجددا تلاش نمایید.");
            }

            if (billing.StartDate > DateTime.Today || DateTime.Today > billing.EndDate)
            {
                var isLastBilling = await _billingRepository.IsLastBilling(billing.FromAccountId, billing.Id);
                if (!isLastBilling)
                    throw new ArgumentValidationException(nameof(command.BillingId), "صورت حساب قابل پرداخت نمی باشد.");
            }

            if (billing.FromAccount.BusinessIdentityId != command.CustomerId) //check from token
            {
                throw new ArgumentValidationException(nameof(command.CustomerId), "صورت حساب مورد نظر به شما تعلق ندارد.");
            }

            var billingRemainAmount = billing.Amount
                                      + billing.PreviousDebitAmount
                                      + billing.PreviousPenaltyAmount
                                      - billing.PreviousCreditAmount
                                      - billing.BillingPayments.Sum(x => x.Amount);

            if (billingRemainAmount < command.Amount)
            {
                throw new ArgumentValidationException(nameof(command.Amount), "مبلغ پرداختی نمی تواند بیشتر از مبلغ فاکتور باشد.");
            }

            return billing;
        }

        private async Task<FinancialDocument> CreateFinancialDocument(List<FinancialDocumentPayment> financialDocumentPayments, int toBusinessIdentityId, CreateBillingPaymentCommand command)
        {
            var tenantPlatformContract = await _tenantPlatformContractRepository.GetByTenantIdAsync(command.TenantId) ?? throw new TenantPlatformContractNotFoundException("قرارداد بین پلتفرم و مالک زیرساخت یافت نشد");
            var financialDocument = new FinancialDocument(command.CustomerId, toBusinessIdentityId, command.TenantId, command.Amount, command.PaymentId,
                FinancialDocumentType.Billing, FinancialDocumentState.Verified, null, FinancialDocumentType.Billing.GetEnumDescription(), tenantPlatformContractId: tenantPlatformContract.Id);
            financialDocument.SetFinancialDocumentPayments(financialDocumentPayments);

            return financialDocument;
        }

        private async Task SetCashWallet(CreateBillingPaymentCommand command)
        {
            var paymentDetail = command.PaymentDetails.FirstOrDefault(x => x.Type != FinancialDocumentPaymentType.Credit && (x.WalletId == null || x.WalletId == 0));
            if (paymentDetail == null)
            {
                return;
            }

            var cashWallet = await _walletRepository.GetCashWalletAsync(command.CustomerId);
            if (cashWallet != null)
            {
                paymentDetail.WalletId = cashWallet?.Id;
                paymentDetail.WalletContractId = cashWallet?.WalletContractId == 0 ? null : cashWallet?.WalletContractId;
            }
        }
    }
}
