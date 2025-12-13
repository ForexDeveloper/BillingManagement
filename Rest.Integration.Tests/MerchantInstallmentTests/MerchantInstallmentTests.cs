using Xunit;
using FluentAssertions;
using Domain.Core.Enums;
using System.Globalization;
using Application.Service.Helper;
using Rest.Integration.Tests.Base;
using Application.Service.Contracts;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.MerchantInstallmentAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.MerchantInstallmentTests;

[Collection(nameof(SharedHostCollection))]
public sealed class MerchantInstallmentTests(SharedHostFixture hostFixture)
{
    private readonly ApplicationDbContext _dbContext = hostFixture.GetMainContext();
    private readonly IMerchantInstallmentService _merchantInstallmentService = hostFixture.GetRequiredService<IMerchantInstallmentService>();

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenFinancialDocumentReceived_ShouldCreateInstallments(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().NotBeNull();
        installments.Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenInstallmentsAreCreated_ShouldAllBeAssignableToInstallment(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllBeAssignableTo<Installment>();
        installments.Should().AllBeOfType<MerchantInstallment>();
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenInstallmentsAreCreated_ShouldSetAllInstallmentTypesEqualToFinancialDocumentType(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installmentType = financialDocumentType == FinancialDocumentType.Purchase
            ? InstallmentType.Purchase
            : InstallmentType.Refund;

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.Type.Should().Be(installmentType));
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenInstallmentsAreCreated_SumOfInstallmentsAmount_ShouldBeEqualToFinancialDocumentAmounts(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.Amount.Should().BePositive());

        installments.Sum(p => p.Amount).Should().Be(financialDocument.Amount);
        installments.Sum(p => p.CashAmount).Should().Be(financialDocument.CashAmount);
        installments.Sum(p => p.CreditAmount).Should().Be(financialDocument.CreditAmount);
        installments.Sum(p => p.PrepaymentAmount).Should().Be(financialDocument.PrepaymentAmount);
        installments.Sum(p => p.CashAmount + p.CreditAmount + p.PrepaymentAmount).Should().Be(financialDocument.Amount);
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenInstallmentsAreCreated_ShouldSetAllAmountsEquallyOnInstallments(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
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

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenBillingPeriodType_Is_Daily_DurationBetweenDueDates_ShouldBeEqualToBillingPeriod(FinancialDocumentType financialDocumentType)
    {
        var pc = new PersianCalendar();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Day);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));
        contract.SetProperty(p => p.BillingPeriod, Random.Shared.Next(1, 100));

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0)
            {
                var depositDate = financialDocument.CreatedDateTime.Date;

                var billingBreak = installment.Type == InstallmentType.Purchase ? contract.BillingBreak!.Value : 0;

                depositDate = pc.AddDays(depositDate, billingBreak);

                var firstDueDate = pc.AddDays(depositDate, contract.BillingPeriod);

                installment.DueDate.Should().Be(firstDueDate);
            }
            else
            {
                var previousInstallment = installments[index - 1];

                var nextDueDate = pc.AddDays(previousInstallment.DueDate, contract.BillingPeriod);

                installment.DueDate.Should().Be(nextDueDate);
            }
        }
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenBillingPeriodType_Is_Weekly_DurationBetweenDueDates_ShouldBeEqualToOneWeek(FinancialDocumentType financialDocumentType)
    {
        var pc = new PersianCalendar();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Week);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0)
            {
                var depositDate = financialDocument.CreatedDateTime.Date;

                var billingBreak = installment.Type == InstallmentType.Purchase ? contract.BillingBreak!.Value : 0;

                depositDate = pc.AddDays(depositDate, billingBreak);

                var firstDueDate = pc.AddWeeks(depositDate, 1);

                installment.DueDate.Should().Be(firstDueDate);
            }
            else
            {
                var previousInstallment = installments[index - 1];

                var nextDueDate = pc.AddWeeks(previousInstallment.DueDate, 1);

                installment.DueDate.Should().Be(nextDueDate);

                var dayOfWeek = pc.GetDayOfWeek(installment.DueDate);

                var previousDayOfWeek = pc.GetDayOfWeek(previousInstallment.DueDate);

                dayOfWeek.Should().Be(previousDayOfWeek);
            }
        }
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenBillingPeriodType_Is_Monthly_DurationBetweenDueDates_ShouldBeEqualToOneMonth(FinancialDocumentType financialDocumentType)
    {
        var pc = new PersianCalendar();

        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.BillingPeriodType, TimeInterval.Month);
        contract.SetProperty(p => p.BillingBreak, Random.Shared.Next(0, 100));

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0)
            {
                var depositDate = financialDocument.CreatedDateTime.Date;

                var billingBreak = installment.Type == InstallmentType.Purchase ? contract.BillingBreak!.Value : 0;

                depositDate = pc.AddDays(depositDate, billingBreak);

                var firstDueDate = pc.AddMonths(depositDate, 1);

                installment.DueDate.Should().Be(firstDueDate);
            }
            else
            {
                var previousInstallment = installments[index - 1];

                var nextDueDate = pc.AddMonths(previousInstallment.DueDate, 1);

                installment.DueDate.Should().Be(nextDueDate);

                var dayOfMonth = pc.GetDayOfMonth(installment.DueDate);

                var previousDayOfMonth = pc.GetDayOfMonth(previousInstallment.DueDate);

                dayOfMonth.Should().Be(previousDayOfMonth);
            }
        }
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenSettlementType_Is_LumpSum_NumberOfInstallments_ShouldBeEqualToOne(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.InstallmentsCount, 1);
        contract.SetProperty(p => p.SettlementType, SettlementType.LumpSum);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments.Should().NotBeNull();
        installments.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenSettlementType_Is_Installments_NumberOfInstallments_ShouldBeEqualToContractInstallmentsCount(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        financialDocument.SetProperty(p => p.Type, financialDocumentType);

        if (financialDocumentType == FinancialDocumentType.Purchase)
        {
            await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);
        }
        else
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);
        }

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        var installmentsCount = contract.InstallmentsCount;

        installments.Should().NotBeNull();
        installments.Should().HaveCount(installmentsCount);
    }

    [Theory]
    [InlineData(CommissionCalculationType.UniformTiered)]
    [InlineData(CommissionCalculationType.CumulativeTiered)]
    public async Task WhenCommissionCalculationType_Is_TieredCommission_ShouldSetZeroCommissionForAllInstallments(CommissionCalculationType commissionCalculationType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionCalculationType, commissionCalculationType);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        var commission = await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
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
        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionReferenceTypes, commissionReferenceTypes.ToList());
        contract.SetProperty(p => p.CommissionCalculationType, CommissionCalculationType.FixedPercentage);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        var commission = await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();

        var installments = await _dbContext.Installments
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

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenCommissionDeductionMethodType_Is_DeductFromFirstInstallment_ShouldSetCommissionOnFirstInstallment(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionDeductionMethodType,
            CommissionDeductionMethodType.DeductFromFirstInstallment);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        var commission = await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();

        if (financialDocumentType == FinancialDocumentType.Refund)
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            commission = await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);

            financialDocument.SetCommission(commission);

            await _dbContext.SaveChangesAsync();
        }

        var installments = await _dbContext.Installments
          .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        installments[0].Commission.Should().BePositive();
        installments[0].Commission.Should().Be(financialDocument.Commission);

        installments.Skip(1).Should().AllSatisfy(p => p.Commission.Should().Be(0));
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
    }

    [Theory]
    [InlineData(FinancialDocumentType.Refund)]
    [InlineData(FinancialDocumentType.Purchase)]
    public async Task WhenCommissionDeductionMethodType_Is_DeductEquallyFromInstallments_ShouldSetCommissionEquallyOnAllInstallments(FinancialDocumentType financialDocumentType)
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        contract.SetProperty(p => p.CommissionDeductionMethodType,
            CommissionDeductionMethodType.DeductEquallyFromInstallments);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        var commission = await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await _dbContext.SaveChangesAsync();

        if (financialDocumentType == FinancialDocumentType.Refund)
        {
            financialDocument = await hostFixture.CreateRefundFinancialDocument(financialDocument);

            commission = await _merchantInstallmentService.CreateRefundInstallments(contract, financialDocument);

            financialDocument.SetCommission(commission);

            await _dbContext.SaveChangesAsync();
        }

        var installments = await _dbContext.Installments
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
}