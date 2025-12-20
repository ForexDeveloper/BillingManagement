using Xunit;
using FluentAssertions;
using Domain.Core.Enums;
using System.Collections;
using System.Globalization;
using Application.Service.Helper;
using Domain.Core.Entities.Shared;
using Rest.Integration.Tests.Base;
using Application.Service.Contracts;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.MerchantBillingTests;

[Collection(nameof(SharedHostCollection))]
public sealed class MerchantBillingTests(SharedHostFixture hostFixture)
{
    private const int SHIFT = 300;

    private readonly ApplicationDbContext _dbContext = hostFixture.GetMainContext();
    private readonly IMerchantBillingService _merchantBillingService = hostFixture.GetRequiredService<IMerchantBillingService>();
    private readonly IMerchantInstallmentService _merchantInstallmentService = hostFixture.GetRequiredService<IMerchantInstallmentService>();

    [Theory]
    [InlineData(TimeInterval.Day, 17)]
    [InlineData(TimeInterval.Week, 3)]
    [InlineData(TimeInterval.Month, 20)]
    public async Task WhenPurchaseInstallmentsAreDetected_ShouldCreateBillings(TimeInterval billingPeriodType, int billingPeriod)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().NotBeNull();
        billings.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task WhenInstallmentsAreDetected_ShouldAllBeAssignableToBilling()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllBeAssignableTo<Billing>();
        billings.Should().AllBeOfType<MerchantBilling>();
    }

    [Fact]
    public async Task WhenInstallmentsAreDetected_AllBillings_ShouldBeNotAbsoluteZero()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.As<MerchantBilling>().IsAbsoluteZero().Should().BeFalse());
    }

    [Fact]
    public async Task WhenPurchaseInstallmentsAreDetected_StatusOfPreviousBillings_ShouldBeEqualToOverdue()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings[^1].Status.Should().BeOneOf(BillingStatus.Issued, BillingStatus.Overdue);
        billings[..^1].Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Overdue));
    }

    [Fact]
    public async Task WhenPurchaseInstallmentsAreDetected_ShouldDebitEachBillingToNextOne()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        foreach (var billing in billings)
        {
            var index = billings.IndexOf(billing);

            if (index == 0)
            {
                billing.Debtor.Should().BeNull();
                billing.DebtorId.Should().BeNull();
            }
            else
            {
                var previousBilling = billings[index - 1];

                billing.Debtor.Should().Be(previousBilling);
                billing.DebtorId.Should().Be(previousBilling.Id);
            }
        }
    }

    [Fact]
    public async Task WhenPurchaseInstallmentsAreDetected_ShouldSetBillingAmountAsNextOnePreviousDebit()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        foreach (var billing in billings)
        {
            var index = billings.IndexOf(billing);

            if (index == 0)
            {
                billing.PreviousDebitAmount.Should().Be(0);
            }
            else
            {
                var previousBilling = billings[index - 1];

                billing.PreviousDebitAmount.Should().Be(previousBilling.Amount);
            }
        }
    }

    [Fact]
    public async Task WhenPurchaseInstallmentsAreDetected_And_PeriodMinCommissionAmount_Is_GreaterThan_PurchaseTransactionsAmount_StatusOfPreviousBillings_ShouldBeEqualToSettled()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.PeriodMinCommissionAmount, 10000000);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings[^1].Status.Should().BeOneOf(BillingStatus.Issued, BillingStatus.Settled);
        billings[..^1].Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Settled));
        billings.Take(billings.Count - 1).Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Settled));
    }

    [Fact]
    public async Task WhenPurchaseInstallmentsAreDetected_And_PeriodMinCommissionAmount_Is_GreaterThan_PurchaseTransactionsAmount_ShouldCreditEachBillingToNextOne()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.PeriodMinCommissionAmount, 10000000);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        foreach (var billing in billings)
        {
            var index = billings.IndexOf(billing);

            if (index == 0)
            {
                billing.Creditor.Should().BeNull();
                billing.CreditorId.Should().BeNull();
            }
            else
            {
                var previousBilling = billings[index - 1];

                billing.Creditor.Should().Be(previousBilling);
                billing.CreditorId.Should().Be(previousBilling.Id);
            }
        }
    }

    [Fact]
    public async Task WhenPurchaseInstallmentsAreDetected_And_PeriodMinCommissionAmount_Is_GreaterThan_PurchaseTransactionsAmount_ShouldSetBillingAmountAsNextOnePreviousCredit()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.PeriodMinCommissionAmount, 10000000);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        foreach (var billing in billings)
        {
            var index = billings.IndexOf(billing);

            if (index == 0)
            {
                billing.PreviousCreditAmount.Should().Be(0);
            }
            else
            {
                var previousBilling = billings[index - 1];

                billing.PreviousCreditAmount.Should().Be(Math.Abs(previousBilling.Amount));
            }
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Daily_DurationBetweenDueDates_ShouldAllBeEqualToBillingPeriod()
    {
        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));
        contract.SetProperty(p => p.BillingPeriod, Random.Shared.Next(1, 100));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today.AddDays(-SHIFT)));

        foreach (var billing in billings)
        {
            var index = billings.IndexOf(billing);

            if (index == billings.Count - 1) continue;

            var nextBilling = billings[index + 1];

            var nextDueDate = pc.AddDays(billing.DueDate, contract.BillingPeriod);

            nextBilling.DueDate.Should().Be(nextDueDate);

            nextBilling.StartDate.Should().Be(billing.DueDate);
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Weekly_DurationBetweenDueDates_ShouldAllBeEqualToOneWeek()
    {
        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 5);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Week);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today.AddDays(-SHIFT)));

        foreach (var billing in billings)
        {
            var index = billings.IndexOf(billing);

            if (index == billings.Count - 1) continue;

            var nextBilling = billings[index + 1];

            var nextDueDate = pc.AddWeeks(billing.DueDate, 1);

            nextBilling.DueDate.Should().Be(nextDueDate);

            nextBilling.StartDate.Should().Be(billing.DueDate);
        }
    }

    [Theory]
    [ClassData(typeof(WeeklyBillingPeriods))]
    public async Task WhenBillingPeriodType_Is_Weekly_AllBillingsDayOfWeek_ShouldBeEqualToBillingPeriod(int billingPeriod)
    {
        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Week);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 6));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today.AddDays(-SHIFT)));

        var periodDayOfWeek = DateHelper.GetPersianDayOfWeek(billingPeriod);

        foreach (var billing in billings)
        {
            var dayOfWeek = pc.GetDayOfWeek(billing.DueDate);

            dayOfWeek.Should().Be(periodDayOfWeek);
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Monthly_DurationBetweenDueDates_ShouldAllBeEqualToOneMonth()
    {
        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 17);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today.AddDays(-SHIFT)));

        foreach (var billing in billings)
        {
            var index = billings.IndexOf(billing);

            if (index == billings.Count - 1) continue;

            var nextBilling = billings[index + 1];

            var nextDueDate = pc.AddMonths(billing.DueDate, 1);

            nextBilling.DueDate.Should().Be(nextDueDate);

            nextBilling.StartDate.Should().Be(billing.DueDate);
        }
    }

    [Theory]
    [ClassData(typeof(MonthlyBillingPeriods))]
    public async Task WhenBillingPeriodType_Is_Monthly_AllBillingsDayOfMonth_ShouldBeEqualToBillingPeriod(int billingPeriod)
    {
        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today.AddDays(-SHIFT)));

        foreach (var billing in billings)
        {
            var dayOfMonth = pc.GetDayOfMonth(billing.DueDate);

            dayOfMonth.Should().Be(billingPeriod);
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Monthly_And_BillingPeriod_Is_30_BillingsDayOfMonth_ShouldBeEqualTo29Or30()
    {
        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 30);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));

        await ConsumePurchaseDocument(contract, 1000);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        foreach (var billing in billings)
        {
            var year = pc.GetYear(billing.DueDate);

            var month = pc.GetMonth(billing.DueDate);

            var daysInMonth = pc.GetDaysInMonth(year, month);

            var dayOfMonth = pc.GetDayOfMonth(billing.DueDate);

            dayOfMonth.Should().Be(daysInMonth == 29 ? 29 : 30);
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Monthly_And_BillingPeriod_Is_31_BillingsDayOfMonth_ShouldBeEqualTo29Or30Or31()
    {
        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 31);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));

        await ConsumePurchaseDocument(contract, 1000);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        foreach (var billing in billings)
        {
            var year = pc.GetYear(billing.DueDate);

            var month = pc.GetMonth(billing.DueDate);

            var daysInMonth = pc.GetDaysInMonth(year, month);

            var dayOfMonth = pc.GetDayOfMonth(billing.DueDate);

            dayOfMonth.Should().Be(daysInMonth);
        }
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17)]
    [InlineData(TimeInterval.Week, 3)]
    [InlineData(TimeInterval.Month, 20)]
    public async Task WhenPurchaseInstallmentsAreDetected_And_BillingBreak_Is_0_NumberOfBillingsWithPositivePurchaseTransactionsAmount_ShouldBeEqualToContractInstallmentsCount(
        TimeInterval billingPeriodType, int billingPeriod)
    {
        await hostFixture.FlushAsync();

        var installmentsCount = Random.Shared.Next(1, 6);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, 0);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);
        contract.SetProperty(p => p.InstallmentsCount, installmentsCount);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.MerchantBillings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Take(installmentsCount).Should().AllSatisfy(p => p.PurchaseTransactionsAmount.Should().BePositive());
        billings.Skip(installmentsCount).Should().AllSatisfy(p => p.PurchaseTransactionsAmount.Should().Be(0));
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.UniformTiered)]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.CumulativeTiered)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.UniformTiered)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.CumulativeTiered)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.UniformTiered)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.CumulativeTiered)]
    public async Task WhenPurchaseInstallmentsAreDetected_And_BillingBreak_Is_0_NumberOfBillingsWithPositivePurchaseTransactionsCommission_ShouldBeEqualToContractInstallmentsCount(
        TimeInterval billingPeriodType, int billingPeriod, CommissionCalculationType commissionCalculationType)
    {
        await hostFixture.FlushAsync();

        List<TieredCommission> tieredCommissions =
        [
            new (0, 5000, 5.7M, null, null),
            new(5001, 10000, 4.7M, null, null),
            new(10001, 20000, 3.7M, null, null),
            new(20001, 40000, 2.7M, null, null),
            new(40001, 80000, 1.7M, null, null),
            new(80001, 200000, 1.2M, null, null),
            new(200001, null, 0.5M, null, null)
        ];

        var installmentsCount = Random.Shared.Next(1, 6);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, 0);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, 0);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);
        contract.SetProperty(p => p.InstallmentsCount, installmentsCount);
        contract.SetProperty(p => p.TieredCommissions, tieredCommissions);
        contract.SetProperty(p => p.CommissionCalculationType, commissionCalculationType);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.MerchantBillings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Take(installmentsCount).Should().AllSatisfy(p => p.PurchaseTransactionsCommission.Should().BePositive());
        billings.Skip(installmentsCount).Should().AllSatisfy(p => p.PurchaseTransactionsCommission.Should().Be(0));
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.FixedPercentage)]
    public async Task WhenRefundInstallmentsAreDetected_And_CommissionCalculationType_Is_FixedAmount_Or_FixedPercentage_NumberOfBillingsWithPositivePurchaseTransactionsCommission_NumberOfBillingsWithPositiveRefundedTransactionsAmount_ShouldBeEqualToContractInstallmentsCount(
        TimeInterval billingPeriodType, int billingPeriod, CommissionCalculationType commissionCalculationType)
    {
        await hostFixture.FlushAsync();

        var installmentsCount = Random.Shared.Next(1, 6);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);
        contract.SetProperty(p => p.InstallmentsCount, installmentsCount);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(20, 100));
        contract.SetProperty(p => p.CommissionCalculationType, commissionCalculationType);

        var purchaseDocument = await ConsumePurchaseDocument(contract);

        await ConsumeRefundDocument(contract, purchaseDocument);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.MerchantBillings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Skip(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsAmount.Should().Be(0));
        billings.Take(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsAmount.Should().BePositive());
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.UniformTiered)]
    [InlineData(TimeInterval.Day, 17, CommissionCalculationType.CumulativeTiered)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.UniformTiered)]
    [InlineData(TimeInterval.Week, 3, CommissionCalculationType.CumulativeTiered)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.UniformTiered)]
    [InlineData(TimeInterval.Month, 20, CommissionCalculationType.CumulativeTiered)]
    public async Task WhenPurchaseInstallmentsAreDetected_And_CommissionCalculationType_Is_UniformTiered_Or_CumulativeTiered_AllOfBillingsRefundedTransactionsCommission_ShouldBeEqualToZero(
        TimeInterval billingPeriodType, int billingPeriod, CommissionCalculationType commissionCalculationType)
    {
        await hostFixture.FlushAsync();

        List<TieredCommission> tieredCommissions =
        [
            new (0, 5000, 5.7M, null, null),
            new(5001, 10000, 4.7M, null, null),
            new(10001, 20000, 3.7M, null, null),
            new(20001, 40000, 2.7M, null, null),
            new(40001, 80000, 1.7M, null, null),
            new(80001, 200000, 1.2M, null, null),
            new(200001, null, 0.5M, null, null)
        ];

        var installmentsCount = Random.Shared.Next(1, 6);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, 0);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, 0);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);
        contract.SetProperty(p => p.InstallmentsCount, installmentsCount);
        contract.SetProperty(p => p.TieredCommissions, tieredCommissions);
        contract.SetProperty(p => p.CommissionCalculationType, commissionCalculationType);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.MerchantBillings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.RefundedTransactionsCommission.Should().Be(0));
        billings.Take(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsCommission.Should().Be(0));
        billings.Skip(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsCommission.Should().Be(0));
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17, 16, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Day, 17, 16, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Week, 3, 7, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Week, 3, 7, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Month, 20, 29, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Month, 20, 29, CommissionCalculationType.FixedPercentage)]
    public async Task WhenRefundInstallmentsAreDetected_And_CommissionCalculationType_Is_FixedAmount_Or_FixedPercentage_And_BillingBreak_Is_GreatEnoughToShiftPurchaseInstallmentsToNextPeriod_StatusOfFirstBilling_ShouldBeEqualToSettled(
        TimeInterval billingPeriodType, int billingPeriod, int billingBreak, CommissionCalculationType commissionCalculationType)
    {
        await hostFixture.FlushAsync();

        var installmentsCount = Random.Shared.Next(1, 6);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, billingBreak);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, 0);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);
        contract.SetProperty(p => p.InstallmentsCount, installmentsCount);
        contract.SetProperty(p => p.CommissionCalculationType, commissionCalculationType);

        var purchaseDocument = await ConsumePurchaseDocument(contract);

        await ConsumeRefundDocument(contract, purchaseDocument);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.MerchantBillings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings[0].Amount.Should().BeNegative();
        billings[0].Status.Should().Be(BillingStatus.Settled);
        billings[0].PurchaseTransactionsAmount.Should().Be(0);
        billings[0].RefundedTransactionsAmount.Should().BePositive();
        billings[0].RefundedTransactionsCommission.Should().BeGreaterThanOrEqualTo(0);

        billings[^1].Amount.Should().Be(0);
        billings[^1].RefundedTransactionsAmount.Should().Be(0);
        billings[^1].RefundedTransactionsCommission.Should().Be(0);
        billings[^1].PurchaseTransactionsAmount.Should().BePositive();
        billings[^1].Status.Should().Be(BillingStatus.Settled);

        billings.Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Settled));
    }

    private async Task ShiftFinancialDocument(FinancialDocument financialDocument, int? shift = null)
    {
        financialDocument.SetProperty(p => p.CreatedDateTime, DateTime.Today.AddDays(-shift ?? -SHIFT));
        financialDocument.SetProperty(p => p.EditDateTime, DateTime.Today.AddDays(-shift ?? -SHIFT));
        await _dbContext.SaveChangesAsync();
    }

    private async Task<FinancialDocument> ConsumePurchaseDocument(TenantMerchantContract contract, int? shift = null)
    {
        var financialDocument = await hostFixture.CreatePurchaseFinancialDocument(contract.Id);

        await ShiftFinancialDocument(financialDocument, shift);

        var commission = await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();

        return financialDocument;
    }

    private async Task ConsumeRefundDocument(TenantMerchantContract contract, FinancialDocument financialDocument, int? shift = null)
    {
        financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

        await ShiftFinancialDocument(financialDocument, shift);

        var commission = await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();
    }
}

public class WeeklyBillingPeriods : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [0];
        yield return [1];
        yield return [2];
        yield return [3];
        yield return [4];
        yield return [5];
        yield return [6];

    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class MonthlyBillingPeriods : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [1];
        yield return [2];
        yield return [3];
        yield return [4];
        yield return [5];
        yield return [6];
        yield return [7];
        yield return [8];
        yield return [9];
        yield return [10];
        yield return [11];
        yield return [12];
        yield return [13];
        yield return [14];
        yield return [15];
        yield return [16];
        yield return [17];
        yield return [18];
        yield return [19];
        yield return [20];
        yield return [21];
        yield return [22];
        yield return [23];
        yield return [24];
        yield return [25];
        yield return [26];
        yield return [27];
        yield return [28];
        yield return [29];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}