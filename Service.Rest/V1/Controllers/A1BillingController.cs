using System.Text.Json;
using Domain.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Application.Service.Helper;
using Application.Service.Contracts;
using Application.Query.ViewModels.Categories;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/merchantBilling")]
[ApiController]
public class A1BillingController(
    ApplicationDbContext dbContext,
    IMerchantInstallmentRepository merchantInstallmentRepository,
    IMerchantBillingService merchantBillingService)
    : ControllerBase
{
    [HttpPost("installment")]
    public async Task<ActionResult<GetCategoryListVm>> SetMerchantInstallments(long tenantMerchantContractId)
    {
        try
        {

            //var contract = await _dbContext.TenantMerchantContracts.FirstOrDefaultAsync(p => p.Id == tenantMerchantContractId);

            //var period = contract.BillingPeriod;

            //var periodType = contract.BillingPeriodType;

            var dictionary = new Dictionary<BillingUniqueKey, int>
            {
                { new BillingUniqueKey(1, 1, 1, TimeInterval.Month, DateTime.Today), 1 },
                { new BillingUniqueKey(1, 1, 2, TimeInterval.Month, DateTime.Today), 2 },
                { new BillingUniqueKey(1, 1, 3, TimeInterval.Month, DateTime.Today), 3 },
                { new BillingUniqueKey(1, 1, 4, TimeInterval.Month, DateTime.Today), 4 }
            };

            var key = dictionary.GetValueOrDefault(new BillingUniqueKey(1, 1, 1, TimeInterval.Month, DateTime.Today));

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

            var wallet = groupWallet.GetValueOrDefault(new WalletKey()
            { TenantId = 1, MerchantId = 2, AccountId = 3, PaymentId = 4 });

            const int amount = 100000;

            const int part = 7;

            var t = amount / part;

            var t1 = amount % part;

            var depositDate = DateTime.Now;

            var ttt = DateTime.Today;

            var installments = new List<MerchantInstallment>();

            var installmentDates = DateHelper.CalculateInstallments(depositDate, 24,
                TimeInterval.Day, 29, 4, TimeInterval.Month);

            var t2 = JsonSerializer.Serialize(installmentDates);

            foreach (var installmentDate in installmentDates)
            {
                var installment = new MerchantInstallment(tenantId: 2, financialDocumentId: 1,
                    fromBusinessIdentityId: 2, toBusinessIdentityId: 3,
                    tenantMerchantContractId: (int)tenantMerchantContractId, amount: 31, number: 2, dueDate: installmentDate,
                    type: InstallmentType.Installment);

                installments.Add(installment);
            }

            await Task.CompletedTask;

            return Ok(installmentDates);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    [HttpPost("billing")]
    public async Task<IActionResult> SetMerchantBilling()
    {
        await Task.CompletedTask;

        return Ok();
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