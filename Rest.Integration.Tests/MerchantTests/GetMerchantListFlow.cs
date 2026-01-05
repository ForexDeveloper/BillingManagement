using Xunit;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Rest.Integration.Tests.Base;
using Application.Query.ViewModels.Merchants;

namespace Rest.Integration.Tests.MerchantTests;

[Collection(nameof(SharedHostCollection))]
public class GetMerchantListFlow(SharedHostFixture hostFixture)
{
    [Fact]
    public async Task GetMerchantList_ShouldReturnListItems()
    {
        // Arrange
        var tenant = hostFixture.Tenant;
        var merchant = hostFixture.Merchant;
        const string requestUri = "api/tenant-panel/merchants";
        var httpClient = await hostFixture.GetAuthenticatedHttpClientAsync(tenant.Id);

        // Act
        var response = await httpClient.GetAsync(requestUri);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<GetMerchantListVm>>(content, hostFixture.SerializerOptions);

        // Assert
        result.Should().NotBeNull();
        result.First().Id.Should().Be(merchant.Id);
        result.First().Title.Should().Be(merchant.Title);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}