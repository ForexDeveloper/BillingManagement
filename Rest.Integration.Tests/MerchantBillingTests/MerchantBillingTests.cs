using Xunit;
using Rest.Integration.Tests.Base;

namespace Rest.Integration.Tests.MerchantBillingTests;

[Collection(nameof(SharedHostCollection))]
public sealed class MerchantBillingTests(SharedHostFixture hostFixture)
{
    [Fact]
    public async Task WhenBillingsAreCreated_ShouldReturnList()
    {
        var merchantBillingService = hostFixture.GetMerchantBillingService();

        await merchantBillingService.IssueOrOverdueBilling(DateTime.Today.AddMonths(-2), CancellationToken.None);
    }
}