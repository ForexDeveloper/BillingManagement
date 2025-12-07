using Domain.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Application.Service.Helper;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Service.Rest.V1.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/merchantBilling")]
public class BillingManualController(
    ApplicationDbContext dbContext,
    IFinancialDocumentRepository financialDocumentRepository,
    ITenantMerchantContractRepository tenantMerchantContractRepository,
    IMerchantInstallmentRepository merchantInstallmentRepository,
    IBackgroundJobService backgroundJobService,
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantBillingService merchantBillingService,
    IMerchantInstallmentService merchantInstallmentService)
    : ControllerBase
{
    [HttpPost("installments")]
    public async Task<ActionResult> SetMerchantInstallments(int tenantId, int merchantId, int financialDocumentId, int day)
    {
        try
        {
            var financialDocument = await financialDocumentRepository.GetByIdAsync(financialDocumentId);

            var contract = await tenantMerchantContractRepository.GetAsync(financialDocument.TenantMerchantContractId!.Value);

            var depositDate = DateTime.Now;

            var installments = new List<MerchantInstallment>();

            var installmentDates = DateHelper.CalculateInstallmentDates(depositDate, 24,
                TimeInterval.Day, 5, 4, TimeInterval.Month);

            for (int i = 0; i < day; i++)
            {
                for (int j = 0; j < 400; j++)
                {
                    var commission = await CreateMerchantInstallments(contract, financialDocument, i);

                    financialDocument.SetCommission(commission);
                }
            }

            //await unitOfWork.SaveChangesAsync();

            return Ok("Installments Created");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("billings")]
    public async Task<IActionResult> ExecuteBillingJobManually(CancellationToken cancellationToken)
    {
        try
        {
            await merchantBillingService.IssueOrOverdueBilling(cancellationToken);

            await backgroundJobService.CreateMerchantBillingJobAsync(cancellationToken);

            return Ok("Billings Created");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<decimal> CreateMerchantInstallments(TenantMerchantContract contract, FinancialDocument financialDocument, int day)
    {
        var today = DateTime.Today.AddDays(-day);

        var financialDocumentTargetAmount = financialDocument.Amount;

        if (contract.CommissionReferenceTypes != null && contract.CommissionReferenceTypes.Any())
        {
            financialDocumentTargetAmount = 0;

            foreach (var contractCommissionReferenceType in contract.CommissionReferenceTypes)
            {
                switch (contractCommissionReferenceType)
                {
                    case CommissionReferenceType.CashAmount:
                        financialDocumentTargetAmount += financialDocument.CashAmount;
                        break;

                    case CommissionReferenceType.CreditAmount:
                        financialDocumentTargetAmount += financialDocument.CreditAmount;
                        break;

                    case CommissionReferenceType.PrepaymentAmount:
                        financialDocumentTargetAmount += financialDocument.PrepaymentAmount;
                        break;

                    case CommissionReferenceType.InterestAmount:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        decimal financialDocumentCommission = 0;

        switch (contract.CommissionCalculationType)
        {
            case CommissionCalculationType.UniformTiered:
            case CommissionCalculationType.CumulativeTiered:
                break;

            case CommissionCalculationType.FixedPercentage:

                if (!contract.FixedPercentageCommission.HasValue) break;

                financialDocumentCommission = financialDocumentTargetAmount * (contract.FixedPercentageCommission.Value / 100);

                financialDocumentCommission = RoundHelper.RoundAmount(financialDocumentCommission);

                if (financialDocumentCommission > contract.TransactionMaxCommissionAmount)
                {
                    financialDocumentCommission = contract.TransactionMaxCommissionAmount.Value;
                }

                if (financialDocumentCommission < contract.TransactionMinCommissionAmount)
                {
                    financialDocumentCommission = contract.TransactionMinCommissionAmount.Value;
                }

                break;

            case CommissionCalculationType.FixedAmount:

                financialDocumentCommission = contract.FixedAmountCommission ?? 0;

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        var installmentCount = contract.InstallmentsCount ?? 1;

        var installmentDates = DateHelper.CalculateInstallmentDates(today, installmentCount,
            TimeInterval.Day, contract.BillingBreak, contract.BillingPeriod, contract.BillingPeriodType);

        var installmentAmount = RoundHelper.RoundAmount(financialDocument.Amount / installmentCount);
        var lastInstallmentAmount = financialDocument.Amount - (installmentAmount * (installmentCount - 1));

        var installmentCashAmount = RoundHelper.RoundAmount(financialDocument.CashAmount / installmentCount);
        var lastInstallmentCashAmount = financialDocument.CashAmount - (installmentCashAmount * (installmentCount - 1));

        var installmentCreditAmount = RoundHelper.RoundAmount(financialDocument.CreditAmount / installmentCount);
        var lastInstallmentCreditAmount = financialDocument.CreditAmount - (installmentCreditAmount * (installmentCount - 1));

        var installmentPrepaymentAmount = RoundHelper.RoundAmount(financialDocument.PrepaymentAmount / installmentCount);
        var lastInstallmentPrepaymentAmount = financialDocument.PrepaymentAmount - (installmentPrepaymentAmount * (installmentCount - 1));

        var installmentCommission = RoundHelper.RoundAmount(financialDocumentCommission / installmentCount);
        var lastInstallmentCommission = financialDocumentCommission - (installmentCommission * (installmentCount - 1));

        List<MerchantInstallment> installments = [];

        for (var i = 0; i < installmentDates.Count; i++)
        {
            decimal amount;
            decimal cashAmount;
            decimal creditAmount;
            decimal prePaymentAmount;
            decimal commission;

            if (i == installmentDates.Count - 1)
            {
                amount = lastInstallmentAmount;
                cashAmount = lastInstallmentCashAmount;
                creditAmount = lastInstallmentCreditAmount;
                prePaymentAmount = lastInstallmentPrepaymentAmount;
                commission = lastInstallmentCommission;
            }
            else
            {
                amount = installmentAmount;
                cashAmount = installmentCashAmount;
                creditAmount = installmentCreditAmount;
                prePaymentAmount = installmentPrepaymentAmount;
                commission = installmentCommission;
            }

            var installmentDate = installmentDates[i];

            var installment = new MerchantInstallment(financialDocument, financialDocument.TenantId,
                financialDocument.TenantId, financialDocument.ToBusinessIdentityId, contract.Id, amount, cashAmount,
                creditAmount, prePaymentAmount, i + 1, installmentDate, InstallmentType.Purchase);

            installments.Add(installment);

            switch (contract.CommissionDeductionMethodType)
            {
                case null:
                case CommissionDeductionMethodType.DeductEquallyFromInstallments:
                    installment.SetCommission(commission);
                    break;

                case CommissionDeductionMethodType.DeductFromFirstInstallment:
                    installments[0].SetCommission(financialDocumentCommission);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await merchantInstallmentRepository.AddRangeAsync(installments);

        return financialDocumentCommission;
    }
}