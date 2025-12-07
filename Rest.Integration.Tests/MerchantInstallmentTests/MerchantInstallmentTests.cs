using Xunit;
using FluentAssertions;
using Domain.Core.Enums;
using System.Globalization;
using Application.Service.Helper;
using Rest.Integration.Tests.Base;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Rest.Integration.Tests.MerchantInstallmentTests;

[Collection(nameof(SharedHostCollection))]
public sealed class MerchantInstallmentTests(SharedHostFixture hostFixture)
{
    [Fact]
    public async Task WhenInstallmentsAreCreated_ShouldSetAllInstallmentTypesToPurchase()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.Type.Should().Be(InstallmentType.Purchase));
    }

    [Fact]
    public async Task WhenInstallmentsAreCreated_SumOfInstallmentsAmount_ShouldBeEqualToFinancialDocumentAmounts()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.Amount.Should().BePositive());

        installments.Sum(p => p.Amount).Should().Be(financialDocument.Amount);
        installments.Sum(p => p.CashAmount).Should().Be(financialDocument.CashAmount);
        installments.Sum(p => p.CreditAmount).Should().Be(financialDocument.CreditAmount);
        installments.Sum(p => p.PrepaymentAmount).Should().Be(financialDocument.PrepaymentAmount);
        installments.Sum(p => p.CashAmount + p.CreditAmount + p.PrepaymentAmount).Should().Be(financialDocument.Amount);
    }

    [Fact]
    public async Task WhenInstallmentsAreCreated_ShouldSetAllAmountsEquallyOnInstallments()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0 || index == installments.Count - 1) continue;

            var previousInstallment = installments[index - 1];

            installment.Amount.Should().Be(previousInstallment.Amount);
            installment.CashAmount.Should().Be(previousInstallment.CashAmount);
            installment.CreditAmount.Should().Be(previousInstallment.CreditAmount);
            installment.PrepaymentAmount.Should().Be(previousInstallment.PrepaymentAmount);
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Daily_DurationBetweenDueDates_ShouldBeEqualToBillingPeriod()
    {
        var pc = new PersianCalendar();

        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.BillingPeriod, Random.Shared.Next(1, 100));

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0) continue;

            var previousInstallment = installments[index - 1];

            var nextDueDate = pc.AddDays(previousInstallment.DueDate, contract.BillingPeriod);

            installment.DueDate.Should().Be(nextDueDate);
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Weekly_DurationBetweenDueDates_ShouldBeEqualToOneWeek()
    {
        var pc = new PersianCalendar();

        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Week);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0) continue;

            var previousInstallment = installments[index - 1];

            var nextDueDate = pc.AddWeeks(previousInstallment.DueDate, 1);

            installment.DueDate.Should().Be(nextDueDate);
        }
    }

    [Fact]
    public async Task WhenBillingPeriodType_Is_Monthly_DurationBetweenDueDates_ShouldBeEqualToOneMonth()
    {
        var pc = new PersianCalendar();

        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0) continue;

            var previousInstallment = installments[index - 1];

            var nextDueDate = pc.AddMonths(previousInstallment.DueDate, 1);

            installment.DueDate.Should().Be(nextDueDate);
        }
    }

    [Fact]
    public async Task WhenSettlementType_Is_LumpSum_NumberOfInstallments_ShouldBeEqualToOne()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.InstallmentsCount, null);
        contract.SetProperty(p => p.SettlementType, SettlementType.LumpSum);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().NotBeNull();
        installments.Should().HaveCount(1);

        installments[0].Commission.Should().BePositive();
        installments[0].Commission.Should().Be(financialDocument.Commission);

        installments.Should().AllSatisfy(p => p.Commission.Should().BePositive());
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
    }

    [Fact]
    public async Task WhenSettlementType_Is_Installments_NumberOfInstallments_ShouldBeEqualToContractInstallmentsCount()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        var installmentsCount = contract.InstallmentsCount ?? 1;

        installments.Should().NotBeNull();
        installments.Should().HaveCount(installmentsCount);
    }

    [Fact]
    public async Task WhenCommissionDeductionMethodType_Is_DeductFromFirstInstallment_ShouldSetCommissionOnFirstInstallment()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionDeductionMethodType, 
            CommissionDeductionMethodType.DeductFromFirstInstallment);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments[0].Commission.Should().BePositive();
        installments[0].Commission.Should().Be(financialDocument.Commission);

        installments.Skip(1).Should().AllSatisfy(p => p.Commission.Should().Be(0));
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
    }

    [Fact]
    public async Task WhenCommissionDeductionMethodType_Is_DeductEquallyFromInstallments_ShouldSetCommissionEquallyOnAllInstallments()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionDeductionMethodType, 
            CommissionDeductionMethodType.DeductEquallyFromInstallments);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.Commission.Should().BePositive());
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0 || index == installments.Count - 1) continue;

            var previousInstallment = installments[index - 1];

            installment.Commission.Should().Be(previousInstallment.Commission);
        }
    }

    [Theory]
    [InlineData(CommissionCalculationType.UniformTiered)]
    [InlineData(CommissionCalculationType.CumulativeTiered)]
    public async Task WhenCommissionCalculationType_Is_TieredCommission_ShouldSetZeroCommissionForAllInstallments(CommissionCalculationType commissionCalculationType)
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionCalculationType, commissionCalculationType);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        financialDocument.Commission.Should().Be(0);

        installments.Should().AllSatisfy(p => p.Commission.Should().Be(0));
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
    }

    [Theory]
    [InlineData(CommissionReferenceType.CashAmount)]
    [InlineData(CommissionReferenceType.CreditAmount)]
    [InlineData(CommissionReferenceType.PrepaymentAmount)]
    [InlineData(CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount)]
    [InlineData(CommissionReferenceType.CashAmount, CommissionReferenceType.PrepaymentAmount)]
    [InlineData(CommissionReferenceType.CreditAmount, CommissionReferenceType.PrepaymentAmount)]
    [InlineData(CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount, CommissionReferenceType.PrepaymentAmount)]
    public async Task WhenCommissionCalculationType_Is_FixedPercentage_And_CommissionReferenceTypes_ContainsItem_ShouldCalculateCommissionBasedOnReferenceTypes(params CommissionReferenceType[] commissionReferenceTypes)
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionReferenceTypes, commissionReferenceTypes.ToList());
        contract.SetProperty(p => p.CommissionCalculationType, CommissionCalculationType.FixedPercentage);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        decimal amount = 0;

        foreach (var commissionReferenceType in commissionReferenceTypes)
        {
            switch (commissionReferenceType)
            {
                case CommissionReferenceType.CashAmount:
                    amount += financialDocument.CashAmount;
                    break;

                case CommissionReferenceType.CreditAmount:
                    amount += financialDocument.CreditAmount;
                    break;

                case CommissionReferenceType.PrepaymentAmount:
                    amount += financialDocument.PrepaymentAmount;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        commission = amount * (contract.FixedPercentageCommission!.Value / 100);

        commission = RoundHelper.RoundAmount(commission);

        if (commission > contract.TransactionMaxCommissionAmount)
        {
            commission = contract.TransactionMaxCommissionAmount.Value;
        }

        if (commission < contract.TransactionMinCommissionAmount)
        {
            commission = contract.TransactionMinCommissionAmount.Value;
        }

        financialDocument.Commission.Should().Be(commission);
        installments.Sum(p => p.Commission).Should().Be(commission);
    }
}