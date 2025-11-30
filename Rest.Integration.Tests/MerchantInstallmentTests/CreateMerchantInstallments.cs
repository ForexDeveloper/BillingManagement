using Xunit;
using FluentAssertions;
using Domain.Core.Enums;
using System.Globalization;
using Rest.Integration.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace Rest.Integration.Tests.MerchantInstallmentTests;

[Collection(nameof(SharedHostCollection))]
public class CreateMerchantInstallments(SharedHostFixture hostFixture)
{
    [Fact]
    public async Task CreateMerchantInstallment_ShouldReturnList()
    {
        var pc = new PersianCalendar();

        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract(
            hostFixture.Tenant.Id,
            hostFixture.Merchant.Id,
            SettlementType.Installments,
            true,
            13,
            CommissionDeductionMethodType.DeductEquallyFromInstallments,
            [
                InterestReferenceType.CashAmount, InterestReferenceType.CreditAmount,
                InterestReferenceType.PrepaymentAmount
            ],
            TimeInterval.Day,
            5,
            DateTime.Today.AddDays(-100),
            0,
            CommissionCalculationType.FixedAmount,
            45000,
            13,
            [
                CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount,
                CommissionReferenceType.InterestAmount, CommissionReferenceType.PrepaymentAmount
            ],
            10000,
            50000,
            100000,
            500000);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreateInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        var installmentsCount = contract.InstallmentsCount ?? 1;

        installments.Should().NotBeNull();
        installments.Should().HaveCount(installmentsCount);

        installments.Should().AllSatisfy(p => p.Amount.Should().BePositive());
        installments.Should().AllSatisfy(p => p.Commission.Should().BePositive());
        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));
        installments.Should().AllSatisfy(p => p.Type.Should().Be(InstallmentType.Purchase));

        installments.Sum(p => p.Amount).Should().Be(financialDocument.Amount);
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
        installments.Sum(p => p.CashAmount).Should().Be(financialDocument.CashAmount);
        installments.Sum(p => p.CreditAmount).Should().Be(financialDocument.CreditAmount);
        installments.Sum(p => p.PrepaymentAmount).Should().Be(financialDocument.PrepaymentAmount);
        installments.Sum(p => p.CashAmount + p.CreditAmount + p.PrepaymentAmount).Should().Be(financialDocument.Amount);

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0) continue;

            var previousInstallment = installments[index - 1];

            DateTime nextDueDate;

            switch (contract.BillingPeriodType)
            {
                case TimeInterval.Day:
                    nextDueDate = pc.AddDays(previousInstallment.DueDate, contract.BillingPeriod);
                    break;

                case TimeInterval.Week:
                    nextDueDate = pc.AddWeeks(previousInstallment.DueDate, 1);
                    break;

                case TimeInterval.Month:
                    nextDueDate = pc.AddMonths(previousInstallment.DueDate, 1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            installment.DueDate.Should().Be(nextDueDate);
        }
    }

    [Fact]
    public async Task CreateMerchantInstallment_ShouldSetCommission()
    {
        var pc = new PersianCalendar();

        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract(
            hostFixture.Tenant.Id,
            hostFixture.Merchant.Id,
            SettlementType.Installments,
            true,
            13,
            CommissionDeductionMethodType.DeductEquallyFromInstallments,
            [
                InterestReferenceType.CashAmount, InterestReferenceType.CreditAmount,
                InterestReferenceType.PrepaymentAmount
            ],
            TimeInterval.Day,
            5,
            DateTime.Today.AddDays(-100),
            0,
            CommissionCalculationType.FixedPercentage,
            45000,
            13,
            [
                CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount,
                CommissionReferenceType.InterestAmount, CommissionReferenceType.PrepaymentAmount
            ],
            10000,
            50000,
            100000,
            500000);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreateInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        var installmentsCount = contract.InstallmentsCount ?? 1;

        installments.Should().NotBeNull();
        installments.Should().HaveCount(installmentsCount);

        installments.Should().AllSatisfy(p => p.Amount.Should().BePositive());
        installments.Should().AllSatisfy(p => p.Commission.Should().BePositive());
        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));
        installments.Should().AllSatisfy(p => p.Type.Should().Be(InstallmentType.Purchase));

        installments.Sum(p => p.Amount).Should().Be(financialDocument.Amount);
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
        installments.Sum(p => p.CashAmount).Should().Be(financialDocument.CashAmount);
        installments.Sum(p => p.CreditAmount).Should().Be(financialDocument.CreditAmount);
        installments.Sum(p => p.PrepaymentAmount).Should().Be(financialDocument.PrepaymentAmount);
        installments.Sum(p => p.CashAmount + p.CreditAmount + p.PrepaymentAmount).Should().Be(financialDocument.Amount);

        foreach (var installment in installments)
        {
            var index = installments.IndexOf(installment);

            if (index == 0) continue;

            var previousInstallment = installments[index - 1];

            DateTime nextDueDate;

            switch (contract.BillingPeriodType)
            {
                case TimeInterval.Day:
                    nextDueDate = pc.AddDays(previousInstallment.DueDate, contract.BillingPeriod);
                    break;

                case TimeInterval.Week:
                    nextDueDate = pc.AddWeeks(previousInstallment.DueDate, 1);
                    break;

                case TimeInterval.Month:
                    nextDueDate = pc.AddMonths(previousInstallment.DueDate, 1);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            installment.DueDate.Should().Be(nextDueDate);
        }
    }

    [Fact]
    public async Task CreateMerchantInstallment_ShouldSetCommissionOnFirstInstallment()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract(
            hostFixture.Tenant.Id,
            hostFixture.Merchant.Id,
            SettlementType.Installments,
            true,
            13,
            CommissionDeductionMethodType.DeductFromFirstInstallment,
            [
                InterestReferenceType.CashAmount, InterestReferenceType.CreditAmount,
                InterestReferenceType.PrepaymentAmount
            ],
            TimeInterval.Day,
            5,
            DateTime.Today.AddDays(-100),
            0,
            CommissionCalculationType.FixedPercentage,
            45000,
            13,
            [
                CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount,
                CommissionReferenceType.InterestAmount, CommissionReferenceType.PrepaymentAmount
            ],
            10000,
            50000,
            100000,
            500000);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreateInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        var installmentsCount = contract.InstallmentsCount ?? 1;

        installments.Should().NotBeNull();
        installments.Should().HaveCount(installmentsCount);

        installments[0].Commission.Should().BePositive();
        installments[0].Commission.Should().Be(financialDocument.Commission);

        installments.Skip(1).Should().AllSatisfy(p => p.Commission.Should().Be(0));
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
    }

    [Fact]
    public async Task CreateMerchantInstallment_ShouldHaveOnlyOneInstallment()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract(
            hostFixture.Tenant.Id,
            hostFixture.Merchant.Id,
            SettlementType.LumpSum,
            true,
            null,
            CommissionDeductionMethodType.DeductFromFirstInstallment,
            [
                InterestReferenceType.CashAmount, InterestReferenceType.CreditAmount,
                InterestReferenceType.PrepaymentAmount
            ],
            TimeInterval.Day,
            5,
            DateTime.Today.AddDays(-100),
            0,
            CommissionCalculationType.FixedPercentage,
            45000,
            13,
            [
                CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount,
                CommissionReferenceType.InterestAmount, CommissionReferenceType.PrepaymentAmount
            ],
            10000,
            50000,
            100000,
            500000);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreateInstallments(contract, financialDocument);

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
    public async Task CreateMerchantInstallment_ShouldSetZeroCommission()
    {
        var dbContext = hostFixture.GetMainContext();

        var contract = await hostFixture.CreateTenantMerchantContract(
            hostFixture.Tenant.Id,
            hostFixture.Merchant.Id,
            SettlementType.Installments,
            true,
            13,
            CommissionDeductionMethodType.DeductFromFirstInstallment,
            [
                InterestReferenceType.CashAmount, InterestReferenceType.CreditAmount,
                InterestReferenceType.PrepaymentAmount
            ],
            TimeInterval.Day,
            5,
            DateTime.Today.AddDays(-100),
            0,
            CommissionCalculationType.CumulativeTiered,
            45000,
            13,
            [
                CommissionReferenceType.CashAmount, CommissionReferenceType.CreditAmount,
                CommissionReferenceType.InterestAmount, CommissionReferenceType.PrepaymentAmount
            ],
            10000,
            50000,
            100000,
            500000);

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        var commission = await merchantInstallmentService.CreateInstallments(contract, financialDocument);

        financialDocument.SetCommission(commission);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments
            .Where(p => p.FinancialDocumentId == financialDocument.Id).ToListAsync();

        var installmentsCount = contract.InstallmentsCount ?? 1;

        financialDocument.Commission.Should().Be(0);

        installments.Should().NotBeNull();
        installments.Should().HaveCount(installmentsCount);
        installments.Should().AllSatisfy(p => p.Commission.Should().Be(0));
        installments.Sum(p => p.Commission).Should().Be(financialDocument.Commission);
    }
}