using Xunit;
using FluentAssertions;
using Domain.Core.Enums;
using Rest.Integration.Tests.Base;
using Application.Service.Contracts;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.MerchantBillingTests;

[Collection(nameof(SharedHostCollection))]
public sealed class MerchantBillingTests(SharedHostFixture hostFixture)
{
    private const int SHIFT = -1000;

    private readonly ApplicationDbContext _dbContext = hostFixture.GetMainContext();
    private readonly IMerchantBillingService _merchantBillingService = hostFixture.GetRequiredService<IMerchantBillingService>();
    private readonly IMerchantInstallmentService _merchantInstallmentService = hostFixture.GetRequiredService<IMerchantInstallmentService>();

    [Fact]
    public async Task WhenInstallmentsAreDetected_ShouldCreateBillings()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        await ShiftFinancialDocument(financialDocument);

        await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().NotBeNull();
        billings.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task WhenInstallmentsAreDetected_AllBillings_ShouldBeNotAbsoluteZero()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        await ShiftFinancialDocument(financialDocument);

        await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllSatisfy(p => p.As<MerchantBilling>().IsAbsoluteZero().Should().BeFalse());
    }

    [Fact]
    public async Task WhenBillingsAreCreated_ShouldAllBeAssignableToBilling()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        await ShiftFinancialDocument(financialDocument);

        await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().AllBeAssignableTo<Billing>();
        billings.Should().AllBeOfType<MerchantBilling>();
    }

    [Fact]
    public async Task WhenBillingsAreCreated_ShouldDebitEachBillingToNextOne()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        await ShiftFinancialDocument(financialDocument);

        await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBillings(CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.MainContractId == contract.Id).ToListAsync();

        billings.Should().NotBeNull();
        billings.Should().HaveCountGreaterThan(0);
        billings[^1].Status.Should().BeOneOf(BillingStatus.Issued, BillingStatus.Overdue);
        billings[..^1].Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Overdue));
        billings.Take(billings.Count - 1).Should().AllSatisfy(p => p.Status.Should().Be(BillingStatus.Overdue));

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
    public async Task WhenBillingsAreCreated_ShouldSetBillingAmountAsNextOnePreviousDebit()
    {
        await hostFixture.FlushAsync();

        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract.Id);

        await ShiftFinancialDocument(financialDocument);

        await _merchantInstallmentService.CreatePurchaseInstallments(contract, financialDocument);

        await _dbContext.SaveChangesAsync();

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

    private async Task ShiftFinancialDocument(FinancialDocument financialDocument)
    {
        financialDocument.SetProperty(p => p.CreatedDateTime, DateTime.Today.AddDays(SHIFT));
        financialDocument.SetProperty(p => p.EditDateTime, DateTime.Today.AddDays(SHIFT));
        await _dbContext.SaveChangesAsync();
    }
}