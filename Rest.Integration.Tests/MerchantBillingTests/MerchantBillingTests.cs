using Xunit;
using Rest.Integration.Tests.Base;
using Application.Service.Contracts;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Rest.Integration.Tests.MerchantBillingTests;

[Collection(nameof(SharedHostCollection))]
public sealed class MerchantBillingTests(SharedHostFixture hostFixture)
{
    private const int SHIFT = -10;
    private readonly DateTime JOB_CREATED_DATETIME = DateTime.Today.AddYears(-10);

    private readonly ApplicationDbContext _dbContext = hostFixture.GetMainContext();
    private readonly IMerchantBillingService _merchantBillingService = hostFixture.GetMerchantBillingService();
    private readonly IMerchantInstallmentService _merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

    [Fact]
    public async Task WhenBillingsAreCreated_ShouldReturnList()
    {
        var contract = await hostFixture.CreateTenantMerchantContract();

        var financialDocument = await hostFixture.CreateFinancialDocument(contract);

        await ShiftFinancialDocument(financialDocument);

        await _merchantInstallmentService.CreateInstallments(contract, financialDocument);

        await _dbContext.SaveChangesAsync();

        await _merchantBillingService.IssueOrOverdueBilling(JOB_CREATED_DATETIME, CancellationToken.None);

        var billings = await _dbContext.Billings.Where(p => p.ContractIds.Contains(contract.Id)).ToListAsync();
    }

    private async Task ShiftFinancialDocument(FinancialDocument financialDocument)
    {
        financialDocument.SetProperty(p => p.CreatedDateTime, DateTime.Today.AddDays(SHIFT));
        financialDocument.SetProperty(p => p.EditDateTime, DateTime.Today.AddDays(SHIFT));
        await _dbContext.SaveChangesAsync();
    }
}