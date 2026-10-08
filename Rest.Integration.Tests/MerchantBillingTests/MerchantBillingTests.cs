using Xunit;
using FluentAssertions;
using Domain.Core.Enums;
using System.Collections;
using System.Globalization;
using Shared.EventBus.Events;
using Application.Service.Helper;
using Domain.Core.Entities.Shared;
using Rest.Integration.Tests.Base;
using Application.Service.Contracts;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.MerchantBillingTests;

[Collection(nameof(SharedHostCollection))]
public sealed class MerchantBillingTests(SharedHostFixture hostFixture)
{
    private int SHIFT = 60;

    private readonly ApplicationDbContext _dbContext = hostFixture.GetMainContext();
    private readonly IBillingPaymentService _billingPaymentService = hostFixture.GetRequiredService<IBillingPaymentService>();
    private readonly IMerchantBillingService _merchantBillingService = hostFixture.GetRequiredService<IMerchantBillingService>();
    private readonly IMerchantInstallmentService _merchantInstallmentService = hostFixture.GetRequiredService<IMerchantInstallmentService>();

    #region Basic

    [Theory]
    [InlineData(TimeInterval.Day, 17)]
    [InlineData(TimeInterval.Week, 3)]
    [InlineData(TimeInterval.Month, 20)]
    public async Task WhenPurchaseInstallmentsAreDetected_ShouldGenerateBillings(TimeInterval billingPeriodType, int billingPeriod)
    {
        OverrideShift(400);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

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

        var billings = await GetAllBillingsAsync(contract.Id);

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

        var billings = await GetAllBillingsAsync(contract.Id);

        billings.Should().AllSatisfy(p => p.As<MerchantBilling>().IsAbsoluteZero().Should().BeFalse());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenPurchaseInstallmentsAreDetected_StatusOfPreviousBillings_ShouldBeEqualToOverdue(bool isCommissionExchanged)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.IsCommissionExchanged, isCommissionExchanged);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id, isCommissionExchanged);

        billings[^1].Status.Should().BeOneOf(BillingStatus.Issued, BillingStatus.Overdue);
        billings[..^1].Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Overdue));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenPurchaseInstallmentsAreDetected_ShouldDebitEachBillingToNextOne(bool isCommissionExchanged)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.IsCommissionExchanged, isCommissionExchanged);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id, isCommissionExchanged);

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenPurchaseInstallmentsAreDetected_ShouldSetBillingPayableAmountAsNextOnePreviousDebit(bool isCommissionExchanged)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.IsCommissionExchanged, isCommissionExchanged);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id, isCommissionExchanged);

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

                billing.PreviousDebitAmount.Should().Be(previousBilling.GetPayableAmount());
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

        var billings = await GetAllBillingsAsync(contract.Id);

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

        var billings = await GetAllBillingsAsync(contract.Id);

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
    public async Task WhenPurchaseInstallmentsAreDetected_And_PeriodMinCommissionAmount_Is_GreaterThan_PurchaseTransactionsAmount_ShouldSetBillingPayableAmountAsNextOnePreviousCredit()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.PeriodMinCommissionAmount, 10000000);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

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

                billing.PreviousCreditAmount.Should().Be(Math.Abs(previousBilling.GetPayableAmount()));
            }
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenBillingPeriodType_Is_Daily_DurationBetweenDueDates_ShouldAllBeEqualToBillingPeriod(bool isCommissionExchanged)
    {
        await hostFixture.FlushAsync();

        var pc = new PersianCalendar();

        var billingPeriod = Random.Shared.Next(1, 10);
        var billingBreak = Random.Shared.Next(0, billingPeriod);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, billingBreak);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.IsCommissionExchanged, isCommissionExchanged);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id, isCommissionExchanged);

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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenBillingPeriodType_Is_Weekly_DurationBetweenDueDates_ShouldAllBeEqualToOneWeek(bool isCommissionExchanged)
    {
        await hostFixture.FlushAsync();

        var pc = new PersianCalendar();

        var billingPeriod = Random.Shared.Next(0, 7);
        var billingBreak = Random.Shared.Next(0, billingPeriod);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, billingBreak);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Week);
        contract.SetProperty(p => p.IsCommissionExchanged, isCommissionExchanged);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id, isCommissionExchanged);

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
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, billingPeriod));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

        billings.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today.AddDays(-SHIFT)));

        var periodDayOfWeek = DateHelper.GetPersianDayOfWeek(billingPeriod);

        foreach (var billing in billings)
        {
            var dayOfWeek = pc.GetDayOfWeek(billing.DueDate);

            dayOfWeek.Should().Be(periodDayOfWeek);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task WhenBillingPeriodType_Is_Monthly_DurationBetweenDueDates_ShouldAllBeEqualToOneMonth(bool isCommissionExchanged)
    {
        OverrideShift(400);

        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 17);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 29));
        contract.SetProperty(p => p.IsCommissionExchanged, isCommissionExchanged);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id, isCommissionExchanged);

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
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 29));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

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
        OverrideShift(1000);

        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 30);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 29));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

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
        OverrideShift(3000);

        var pc = new PersianCalendar();

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 31);
        contract.SetProperty(p => p.InstallmentsCount, 120);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 29));

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

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
        OverrideShift(400);

        await hostFixture.FlushAsync();

        var installmentsCount = Random.Shared.Next(1, 6);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, 0);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);
        contract.SetProperty(p => p.InstallmentsCount, installmentsCount);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllMerchantBillingsAsync(contract.Id);

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
        OverrideShift(400);

        await hostFixture.FlushAsync();

        List<TieredCommission> tieredCommissions =
        [
            new(0, 5000, 5.7M, null, null),
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

        var billings = await GetAllMerchantBillingsAsync(contract.Id);

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
        OverrideShift(400);

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

        var billings = await GetAllMerchantBillingsAsync(contract.Id);

        billings.Take(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsAmount.Should().BePositive());
        billings.Skip(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsAmount.Should().Be(0));
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
        OverrideShift(400);

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

        var billings = await GetAllMerchantBillingsAsync(contract.Id);

        billings.Should().AllSatisfy(p => p.RefundedTransactionsCommission.Should().Be(0));
        billings.Take(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsCommission.Should().Be(0));
        billings.Skip(installmentsCount).Should().AllSatisfy(p => p.RefundedTransactionsCommission.Should().Be(0));
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17, 17, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Day, 17, 17, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Week, 3, 7, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Week, 3, 7, CommissionCalculationType.FixedPercentage)]
    [InlineData(TimeInterval.Month, 20, 30, CommissionCalculationType.FixedAmount)]
    [InlineData(TimeInterval.Month, 20, 30, CommissionCalculationType.FixedPercentage)]
    public async Task WhenRefundInstallmentsAreDetected_And_CommissionCalculationType_Is_FixedAmount_Or_FixedPercentage_And_BillingBreak_Is_GreatEnoughToShiftPurchaseInstallmentsToNextPeriod_StatusOfFirstBilling_ShouldBeEqualToSettled(
        TimeInterval billingPeriodType, int billingPeriod, int billingBreak, CommissionCalculationType commissionCalculationType)
    {
        OverrideShift(250);

        await hostFixture.FlushAsync();

        var installmentsCount = Random.Shared.Next(1, 5);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingBreak, billingBreak);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);
        contract.SetProperty(p => p.InstallmentsCount, installmentsCount);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, decimal.Zero);
        contract.SetProperty(p => p.CommissionCalculationType, commissionCalculationType);

        var purchaseDocument = await ConsumePurchaseDocument(contract);

        await ConsumeRefundDocument(contract, purchaseDocument);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllMerchantBillingsAsync(contract.Id);

        billings[0].Amount.Should().BeNegative();
        billings[0].PurchaseTransactionsAmount.Should().Be(0);
        billings[0].PurchaseTransactionsCommission.Should().Be(0);
        billings[0].RefundedTransactionsAmount.Should().BePositive();
        billings[0].RefundedTransactionsCommission.Should().BePositive();

        billings[installmentsCount].Amount.Should().Be(0);
        billings[installmentsCount].RefundedTransactionsAmount.Should().Be(0);
        billings[installmentsCount].RefundedTransactionsCommission.Should().Be(0);
        billings[installmentsCount].PurchaseTransactionsAmount.Should().BePositive();
        billings[installmentsCount].PurchaseTransactionsCommission.Should().BePositive();

        billings[installmentsCount + 1].PreviousCreditAmount.Should().Be(0);

        billings.Take(installmentsCount).Should().AllSatisfy(p => p.Amount.Should().BeNegative());
        billings.Skip(installmentsCount + 1).Should().AllSatisfy(p => p.IsAbsoluteZero().Should().BeTrue());
        billings.Take(installmentsCount + 1).Should().AllSatisfy(p => p.IsAbsoluteZero().Should().BeFalse());
        billings.Skip(1).Take(installmentsCount).Should().AllSatisfy(p => p.PreviousCreditAmount.Should().BePositive());

        billings.Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Settled));

        billings.Skip(1).Take(installmentsCount - 1).Should()
            .AllSatisfy(p => p.PreviousCreditAmount.Should().BePositive()).And
            .AllSatisfy(p => p.PurchaseTransactionsAmount.Should().BePositive()).And
            .AllSatisfy(p => p.RefundedTransactionsAmount.Should().BePositive()).And
            .AllSatisfy(p => p.PurchaseTransactionsCommission.Should().BePositive()).And
            .AllSatisfy(p => p.RefundedTransactionsCommission.Should().BePositive());
    }

    #endregion


    #region Advance

    [Fact]
    public async Task WhenBillingPeriodType_Is_Daily_And_BillingPeriod_Is_Equal2Today_And_NoInstallmentsDetected_ShouldCreateFirstBilling()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetFirstBillingAsync(contract.Id);

        billing.Should().NotBeNull();
        billing.DueDate.Should().Be(DateTime.Today);
        billing.Status.Should().Be(BillingStatus.Settled);
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Weekly_And_BillingPeriod_Is_Equal2Today_And_NoInstallmentsDetected_ShouldCreateFirstBilling()
    {
        await hostFixture.FlushAsync();

        var pc = new PersianCalendar();

        var dayOfWeek = pc.GetDayOfWeek(DateTime.Today);

        var billingPeriod = MapDayOfWeekToBillingPeriod(dayOfWeek);

        var periodDayOfWeek = DateHelper.GetPersianDayOfWeek(billingPeriod);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Week);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetFirstBillingAsync(contract.Id);

        billing.Should().NotBeNull();
        billing.DueDate.Should().Be(DateTime.Today);
        billing.Status.Should().Be(BillingStatus.Settled);
        pc.GetDayOfWeek(billing.DueDate).Should().Be(periodDayOfWeek);
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Monthly_And_BillingPeriod_Is_Equal2Today_And_NoInstallmentsDetected_ShouldCreateFirstBilling()
    {
        await hostFixture.FlushAsync();

        var pc = new PersianCalendar();

        var billingPeriod = pc.GetDayOfMonth(DateTime.Today);

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetFirstBillingAsync(contract.Id);

        billing.Should().NotBeNull();
        billing.DueDate.Should().Be(DateTime.Today);
        billing.Status.Should().Be(BillingStatus.Settled);
        pc.GetDayOfMonth(billing.DueDate).Should().Be(billingPeriod);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    public async Task WhenBillingPeriod_Is_Equal2Today_And_NoInstallmentsDetected_AmountOfDefaultBilling_ShouldBeEqualToPeriodMinCommissionAmount(decimal periodMinCommissionAmount)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, periodMinCommissionAmount);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetFirstBillingAsync(contract.Id);

        billing.Should().NotBeNull();

        if (periodMinCommissionAmount == 0)
        {
            billing.Amount.Should().Be(0);
            billing.As<MerchantBilling>().IsAbsoluteZero().Should().BeTrue();
        }
        else
        {
            billing.Amount.Should().BeNegative();
            billing.As<MerchantBilling>().IsAbsoluteZero().Should().BeFalse();
        }
    }

    #endregion


    #region Payment

    [Fact]
    public async Task WhenSetFullPaymentForBilling_BillingStatus_ShouldBeEqualToSettled()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        await _billingPaymentService.SetMerchantBillingPayment(new PmBillingManualPaymentUpdateStateEvent()
        {
            PaymentId = 12,
            BillingId = billing!.Id,
            TenantId = billing.TenantId,
            PaymentDate = DateTime.Today,
            Amount = billing.GetPayableAmount()
        });

        billing.GetPayableAmount().Should().Be(0);
        billing.Status.Should().Be(BillingStatus.Settled);
    }

    [Fact]
    public async Task WhenSetPartialPaymentForBilling_BillingStatus_ShouldBeEqualToPartiallyPaid()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var oldPayableAmount = billing!.GetPayableAmount();

        var paymentAmount = billing.GetPayableAmount() / 2;

        await _billingPaymentService.SetMerchantBillingPayment(new PmBillingManualPaymentUpdateStateEvent()
        {
            PaymentId = 12,
            Amount = paymentAmount,
            BillingId = billing.Id,
            TenantId = billing.TenantId,
            PaymentDate = DateTime.Today
        });

        billing.Status.Should().Be(BillingStatus.PartiallyPaid);
        billing.GetPayableAmount().Should().Be(oldPayableAmount - paymentAmount);
    }

    #endregion


    #region Additions

    [Fact]
    public async Task WhenSetAdditionsForBilling_NewPayableAmount_ShouldBeGreaterThanOrEqualToOldPayableAmount()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var oldPayableAmount = billing!.GetPayableAmount();

        var additionsAmount = Random.Shared.Next(0, 2000);

        billing.SetAdditions(additionsAmount);

        var newPayableAmount = billing.GetPayableAmount();

        billing.AdditionsAmount.Should().Be(additionsAmount);
        newPayableAmount.Should().BeGreaterThanOrEqualTo(oldPayableAmount);
    }

    [Fact]
    public async Task WhenSetAdditionsForBilling_If_NewPayableAmount_Is_Negative_ShouldThrowArgumentValidationException()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var additionsAmount = billing!.GetPayableAmount();

        var deductionsAmount = billing.GetPayableAmount() * 2;

        billing.SetAdditions(additionsAmount);

        billing.SetDeductions(deductionsAmount);

        Assert.Throws<ArgumentValidationException>(() => billing.SetAdditions(0));
    }

    [Fact]
    public async Task WhenSetAdditionsForBilling_If_PaymentDeadlineDate_Is_LessThanToday_ShouldThrowArgumentValidationException()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

        billings[..^1].Should().AllSatisfy(p => Assert.Throws<ArgumentValidationException>(() => p.SetAdditions(1000)));
    }

    [Fact]
    public async Task WhenSetAdditionsForSettledBilling_BillingStatus_ShouldBeEqualToPartiallyPaid()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        await _billingPaymentService.SetMerchantBillingPayment(new PmBillingManualPaymentUpdateStateEvent()
        {
            PaymentId = 12,
            BillingId = billing!.Id,
            TenantId = billing.TenantId,
            PaymentDate = DateTime.Today,
            Amount = billing.GetPayableAmount()
        });

        billing.SetAdditions(1000);

        billing.Status.Should().Be(BillingStatus.PartiallyPaid);
    }

    [Fact]
    public async Task WhenSetAdditionsForBilling_If_PayableAmount_Is_Zero_Or_Billing_Is_AbsoluteZero_BillingStatus_ShouldBeEqualToIssued()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var payableAmount = billing!.GetPayableAmount();

        billing.SetDeductions(payableAmount);
        billing.GetPayableAmount().Should().Be(0);
        billing.Status.Should().Be(BillingStatus.Settled);

        billing.SetAdditions(payableAmount);
        billing.Status.Should().Be(BillingStatus.Issued);
        billing.GetPayableAmount().Should().BePositive();
    }

    [Fact]
    public async Task WhenSetAdditionsForBilling_If_PayableAmount_Is_Negative_And_AdditionsAmount_Is_EqualToAbsoluteOfPayableAmount_BillingStatus_ShouldBeEqualToSettled()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, 10000000);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var additionsAmount = Math.Abs(billing!.GetPayableAmount());

        billing.SetAdditions(additionsAmount);

        billing.GetPayableAmount().Should().Be(0);
        billing.Status.Should().Be(BillingStatus.Settled);
    }

    [Fact]
    public async Task WhenSetAdditionsForBilling_If_PayableAmount_Is_Negative_And_AdditionsAmount_Is_LessThanAbsoluteOfPayableAmount_BillingStatus_ShouldBeEqualToSettled()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, 10000000);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var additionsAmount = Math.Abs(billing!.GetPayableAmount()) / 2;

        billing.SetAdditions(additionsAmount);

        billing.GetPayableAmount().Should().BeNegative();
        billing.Status.Should().Be(BillingStatus.Settled);
    }

    [Fact]
    public async Task WhenSetAdditionsForBilling_If_PayableAmount_Is_Negative_And_AdditionsAmount_Is_GreaterThanAbsoluteOfPayableAmount_BillingStatus_ShouldBeEqualToIssued()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.PeriodMinCommissionAmount, 10000000);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var additionsAmount = Math.Abs(billing!.GetPayableAmount()) * 2;

        billing.SetAdditions(additionsAmount);

        billing.Status.Should().Be(BillingStatus.Issued);
        billing.GetPayableAmount().Should().BePositive();
    }

    #endregion


    #region Deductions

    [Fact]
    public async Task WhenSetDeductionsForBilling_NewPayableAmount_ShouldBeLessThanOrEqualToOldPayableAmount()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var oldPayableAmount = billing!.GetPayableAmount();

        var deductionsAmount = Random.Shared.Next(0, (int)oldPayableAmount);

        billing.SetDeductions(deductionsAmount);

        var newPayableAmount = billing.GetPayableAmount();

        billing.DeductionsAmount.Should().Be(deductionsAmount);
        newPayableAmount.Should().BeLessThanOrEqualTo(oldPayableAmount);
    }

    [Fact]
    public async Task WhenSetDeductionsForBilling_If_PaymentDeadlineDate_Is_LessThanToday_ShouldThrowArgumentValidationException()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

        billings[..^1].Should().AllSatisfy(p => Assert.Throws<ArgumentValidationException>(() => p.SetDeductions(1000)));
    }

    [Fact]
    public async Task WhenSetDeductionsForBilling_If_DeductionsAmount_Is_GreaterThanPayableAmount_ShouldThrowArgumentValidationException()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var payableAmount = billing!.GetPayableAmount();

        var deductionsAmount = Random.Shared.Next((int)payableAmount + 1, (int)payableAmount * 2);

        Assert.Throws<ArgumentValidationException>(() => billing.SetDeductions(deductionsAmount));
    }

    [Fact]
    public async Task WhenSetDeductionsForBilling_If_DeductionsAmount_Is_EqualToPayableAmount_BillingStatus_ShouldBeEqualToSettled()
    {
        OverrideShift(24);

        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriod, 1);
        contract.SetProperty(p => p.InstallmentsCount, 24);
        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billing = await GetLastBillingAsync(contract.Id);

        var deductionsAmount = billing!.GetPayableAmount();

        billing.SetDeductions(deductionsAmount);

        billing.GetPayableAmount().Should().Be(0);
        billing.Status.Should().Be(BillingStatus.Settled);
    }

    #endregion

    #region IsCommissionExchanged

    [Theory]
    [InlineData(TimeInterval.Day, 17)]
    [InlineData(TimeInterval.Week, 3)]
    [InlineData(TimeInterval.Month, 20)]
    public async Task WhenPurchaseInstallmentsAreDetected_And_IsCommissionExchange_Is_False_HalfOfBillings_ShouldBeTenantToMerchant_HalfOfBillings_ShouldBeMerchantToTenant(
            TimeInterval billingPeriodType, int billingPeriod)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.IsCommissionExchanged, false);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

        var totalCount = billings.Count;

        billings.Count(p => p.Type == BillingType.TenantToMerchant).Should().Be(totalCount / 2);
        billings.Count(p => p.Type == BillingType.MerchantToTenant).Should().Be(totalCount / 2);
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17)]
    [InlineData(TimeInterval.Week, 3)]
    [InlineData(TimeInterval.Month, 20)]
    public async Task WhenPurchaseInstallmentsAreDetected_And_IsCommissionExchange_Is_False_AllBillings_ShouldContainPairId(
        TimeInterval billingPeriodType, int billingPeriod)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.IsCommissionExchanged, false);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

        billings.Should().AllSatisfy(p => p.PairId.Should().NotBeNull());
    }

    [Theory]
    [InlineData(TimeInterval.Day, 17)]
    [InlineData(TimeInterval.Week, 3)]
    [InlineData(TimeInterval.Month, 20)]
    public async Task WhenPurchaseInstallmentsAreDetected2_And_IsCommissionExchange_Is_False_PayableAmountOfMerchantToTenantBillings_ShouldBeEqualTo(
        TimeInterval billingPeriodType, int billingPeriod)
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.IsCommissionExchanged, false);
        contract.SetProperty(p => p.BillingPeriod, billingPeriod);
        contract.SetProperty(p => p.BillingPeriodType, billingPeriodType);

        await ConsumePurchaseDocument(contract);

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await GetAllBillingsAsync(contract.Id);

        billings.Should().AllSatisfy(p => p.PairId.Should().NotBeNull());
    }

    #endregion

    private void OverrideShift(int shift)
    {
        SHIFT = shift;
    }

    private async Task<Billing?> GetLastBillingAsync(int contractId)
    {
        return await _dbContext.Billings.Where(p => p.MainContractId == contractId).OrderByDescending(p => p.DueDate).FirstOrDefaultAsync();
    }

    private async Task<Billing?> GetFirstBillingAsync(int contractId)
    {
        return await _dbContext.Billings.Where(p => p.MainContractId == contractId).OrderBy(p => p.DueDate).FirstOrDefaultAsync();
    }

    private async Task<List<Billing>> GetAllBillingsAsync(int contractId, bool? isCommissionExchanged = null)
    {
        var query = _dbContext.Billings.Where(p => p.MainContractId == contractId).OrderBy(p => p.DueDate);

        if (!isCommissionExchanged.HasValue)
        {
            return await query.ToListAsync();
        }

        if (isCommissionExchanged.Value)
        {
            return await query.Where(p => p.Type == BillingType.TenantToMerchant).ToListAsync();
        }

        return await query.Where(p => p.Type == BillingType.MerchantToTenant).ToListAsync();
    }

    private async Task<List<MerchantBilling>> GetAllMerchantBillingsAsync(int contractId)
    {
        return await _dbContext.MerchantBillings.Where(p => p.MainContractId == contractId).OrderBy(p => p.DueDate).ToListAsync();
    }

    private async Task ShiftFinancialDocument(FinancialDocument financialDocument)
    {
        financialDocument.SetProperty(p => p.CreatedDateTime, DateTime.Today.AddDays(-SHIFT));
        financialDocument.SetProperty(p => p.EditDateTime, DateTime.Today.AddDays(-SHIFT));
        await _dbContext.SaveChangesAsync();
    }

    private async Task<FinancialDocument> ConsumePurchaseDocument(TenantMerchantContract contract)
    {
        var financialDocument = await hostFixture.CreatePurchaseFinancialDocument(contract.Id);

        await ShiftFinancialDocument(financialDocument);

        var commission = await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();

        return financialDocument;
    }

    private async Task ConsumeRefundDocument(TenantMerchantContract contract, FinancialDocument financialDocument)
    {
        financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

        await ShiftFinancialDocument(financialDocument);

        var commission = await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();
    }

    private static int MapDayOfWeekToBillingPeriod(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Saturday => 0,
            DayOfWeek.Sunday => 1,
            DayOfWeek.Monday => 2,
            DayOfWeek.Tuesday => 3,
            DayOfWeek.Wednesday => 4,
            DayOfWeek.Thursday => 5,
            DayOfWeek.Friday => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
        };
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