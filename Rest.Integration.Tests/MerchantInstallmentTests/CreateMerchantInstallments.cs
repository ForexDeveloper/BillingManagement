using Xunit;
using FluentAssertions;
using Domain.Core.Enums;
using Rest.Integration.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace Rest.Integration.Tests.MerchantInstallmentTests;

[Collection(nameof(SharedHostCollection))]
public class CreateMerchantInstallments(SharedHostFixture hostFixture)
{
    [Fact]
    public async Task CreateMerchantInstallment_ShouldReturnList()
    {
        var dbContext = hostFixture.GetMainContext();

        var merchantInstallmentService = hostFixture.GetMerchantInstallmentService();

        await merchantInstallmentService.CreateInstallments(hostFixture.TenantMerchantContract, hostFixture.FinancialDocument);

        await dbContext.SaveChangesAsync();

        var installments = await dbContext.MerchantInstallments.ToListAsync();

        var installmentsCount = hostFixture.TenantMerchantContract.InstallmentsCount ?? 1;

        installments.Should().NotBeNull();
        installments.Should().HaveCount(installmentsCount);
        installments.Should().AllSatisfy(p => p.Amount.Should().BePositive());
        installments.Should().AllSatisfy(p => p.DueDate.Should().BeAfter(DateTime.Today));
        installments.Should().AllSatisfy(p => p.Type.Should().Be(InstallmentType.Purchase));
    }
}