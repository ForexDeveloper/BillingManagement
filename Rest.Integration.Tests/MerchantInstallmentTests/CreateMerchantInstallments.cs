using Xunit;
using Rest.Integration.Tests.Base;

namespace Rest.Integration.Tests.MerchantInstallmentTests;

[Collection(nameof(SharedHostCollection))]
public class CreateMerchantInstallments(SharedHostFixture hostFixture)
{
    [Fact]
    public async Task CreateMerchantInstallment_ShouldReturnList()
    {

    }
}