using Application.Query.ViewModels.Categories;
using Application.Service.Contracts;
using Application.Service.Helper;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Infrastructure.Data.Repository.EfCore.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using System.Text.Json;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/merchantBilling")]
[ApiController]
public class A1BillingController(
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
    [HttpPost("installment")]
    public async Task<ActionResult<GetCategoryListVm>> SetMerchantInstallments(int tenantId, int merchantId, int financialDocumentId)
    {
        try
        {
            var today = DateTime.Today;

            var pc = new PersianCalendar();

            var year = pc.GetYear(today);
            var month = pc.GetMonth(today);
            var dayOfMonth = pc.GetDayOfMonth(today);

            var newDateTimePc = new DateTime(year, month, dayOfMonth, pc);

            var pcToDateTime = pc.ToDateTime(year, month, dayOfMonth, 0, 0, 0, 0);

            var shiftDay = pc.AddDays(new DateTime(year, month, dayOfMonth, pc), 1);
            var shiftWeek = pc.AddWeeks(new DateTime(year, month, dayOfMonth, pc), 1);
            var shiftMonth = pc.AddMonths(new DateTime(year, month, dayOfMonth, pc), 1);

            var shiftDay1 = pc.AddDays(today, 1);
            var shiftWeek1 = pc.AddWeeks(today, 1);
            var shiftMonth1 = pc.AddMonths(today, 1);

            var financialDocument = await financialDocumentRepository.GetByIdAsync(financialDocumentId);

            var contract = await tenantMerchantContractRepository.GetAsync(financialDocument.TenantMerchantContractId!.Value);

            var depositDate = DateTime.Now;

            var installments = new List<MerchantInstallment>();

            var installmentDates = DateHelper.CalculateInstallments(depositDate, 24,
                TimeInterval.Day, 5, 4, TimeInterval.Month);

            var serializeDates = JsonSerializer.Serialize(installmentDates);

            //var contract = await tenantMerchantContractRepository.GetActiveContractAsync(contract.TenantId, contract.MerchantId);

            var commission = await CreateMerchantInstallments(contract, financialDocument);

            financialDocument.SetCommission(commission);

            await unitOfWork.SaveChangesAsync();

            var wallets = new List<WalletDto>
            {
                new()
                {
                    TenantId = 1,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 1,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 3,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 2,
                    AccountId = 4,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 3,
                    MerchantId = 2,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 1,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 1,
                    AccountId = 2,
                    PaymentId = 3
                },
                new()
                {
                    TenantId = 1,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 1,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 3,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 2,
                    AccountId = 4,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 3,
                    MerchantId = 2,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 1,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 1,
                    AccountId = 2,
                    PaymentId = 3
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 1,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 3,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 2,
                    AccountId = 4,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 3,
                    MerchantId = 2,
                    AccountId = 1,
                    PaymentId = 4
                },    new()
                {
                    TenantId = 2,
                    MerchantId = 1,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 3,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 2,
                    AccountId = 4,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 1
                },   new()
                {
                    TenantId = 2,
                    MerchantId = 3,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 2,
                    AccountId = 4,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 3,
                    MerchantId = 2,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 1,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 1,
                    AccountId = 2,
                    PaymentId = 3
                },
                new()
                {
                    TenantId = 1,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 1,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 3,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 2,
                    AccountId = 4,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 2,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 2,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 1
                },  new()
                {
                    TenantId = 3,
                    MerchantId = 2,
                    AccountId = 1,
                    PaymentId = 4
                },  new()
                {
                    TenantId = 1,
                    MerchantId = 4,
                    AccountId = 3,
                    PaymentId = 3
                },  new()
                {
                    TenantId = 4,
                    MerchantId = 1,
                    AccountId = 2,
                    PaymentId = 3
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 1,
                    AccountId = 3,
                    PaymentId = 4
                },
                new()
                {
                    TenantId = 2,
                    MerchantId = 3,
                    AccountId = 1,
                    PaymentId = 4
                }
            };

            var groupWallet = wallets.GroupBy(p => new WalletKey()
            {
                TenantId = p.TenantId,
                MerchantId = p.MerchantId,
                AccountId = p.AccountId,
                PaymentId = p.PaymentId
            }).ToDictionary(p => p.Key, p => p);

            var wallet = groupWallet.GetValueOrDefault(new WalletKey() { TenantId = 1, MerchantId = 2, AccountId = 3, PaymentId = 4 });

            return Ok("Installments Created");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    [HttpPost("billing")]
    public async Task<IActionResult> ExecuteBillingJobManually(CancellationToken cancellationToken)
    {
        try
        {
            var jobCreatedDateTime = await backgroundJobService.CreateMerchantBillingJobAsync(cancellationToken);

            await merchantBillingService.IssueOrOverdueBilling(jobCreatedDateTime, cancellationToken);

            return Ok("Billings Created");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<decimal> CreateMerchantInstallments(TenantMerchantContract contract, FinancialDocument financialDocument)
    {
        var today = DateTime.Today.AddDays(-3);

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
                break;

            case CommissionCalculationType.CumulativeTiered:

                var (startOfPeriod, _) = ContractPeriodHelper.GetPeriodBySpecificDate(contract.BillingPeriod,
                    contract.BillingPeriodType, contract.DailyBillingOriginDate, today);

                var sumOfTieredTransactions = await merchantInstallmentRepository.GetSumOfTieredTransactionsInSpecificPeriod(contract,
                    startOfPeriod, today);

                if (contract.TieredCommissions == null) break;

                var tieredCommission = contract.TieredCommissions.FirstOrDefault(p =>
                    p.FromAmount < sumOfTieredTransactions && sumOfTieredTransactions <= p.ToAmount);

                if (tieredCommission == null)
                {
                    var minTieredCommission = contract.TieredCommissions.MinBy(p => p.ToAmount);

                    var maxTieredCommission = contract.TieredCommissions.MaxBy(p => p.ToAmount);

                    if (sumOfTieredTransactions <= minTieredCommission.FromAmount)
                    {
                        tieredCommission = minTieredCommission;
                    }

                    else if (sumOfTieredTransactions > maxTieredCommission.ToAmount)
                    {
                        tieredCommission = maxTieredCommission;
                    }

                    if (tieredCommission == null) break;

                    financialDocumentCommission = financialDocumentTargetAmount * (tieredCommission.Percentage / 100);

                    if (financialDocumentCommission > tieredCommission.MaxAmount)
                    {
                        financialDocumentCommission = tieredCommission.MaxAmount.Value;
                    }

                    if (financialDocumentCommission < tieredCommission.MinAmount)
                    {
                        financialDocumentCommission = tieredCommission.MinAmount.Value;
                    }
                }

                break;

            case CommissionCalculationType.FixedPercentage:

                if (!contract.FixedPercentageCommission.HasValue) break;

                financialDocumentCommission = financialDocumentTargetAmount * (contract.FixedPercentageCommission.Value / 100);

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

        var installmentDates = DateHelper.CalculateInstallments(today, contract.InstallmentsCount,
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

public class WalletDto
{
    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public int AccountId { get; set; }

    public int PaymentId { get; set; }
}

public record WalletKey
{
    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public int AccountId { get; set; }

    public int PaymentId { get; set; }
}

public sealed record BillingUniqueKey
{
    public int TenantId { get; set; }

    public int MerchantId { get; set; }

    public int BillingPeriod { get; set; }

    public TimeInterval BillingPeriodType { get; set; }

    public DateTime? DailyBillingOriginDate { get; set; }

    public BillingUniqueKey(int tenantId, int merchantId, int billingPeriod, TimeInterval billingPeriodType, DateTime? dailyBillingOriginDate)
    {
        TenantId = tenantId;
        MerchantId = merchantId;
        BillingPeriod = billingPeriod;
        BillingPeriodType = billingPeriodType;
        DailyBillingOriginDate = dailyBillingOriginDate;
    }
}