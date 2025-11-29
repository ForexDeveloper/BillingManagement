using Xunit;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using Rest.Integration.Tests.Base;
using Application.Query.ViewModels.Merchants;

namespace Rest.Integration.Tests.BillingTests;

[Collection(nameof(SharedHostCollection))]
public class GetMerchantListFlow
{
    private readonly SharedHostFixture _hostFixture;

    public GetMerchantListFlow(SharedHostFixture hostFixture)
    {
        _hostFixture = hostFixture;
    }

    [Fact]
    public async Task GetMerchantList_ShouldReturnListItems()
    {
        // Arrange
        var tenant = _hostFixture.Tenant;
        var merchant = _hostFixture.Merchant;
        const string requestUri = "api/tenant-panel/merchants";
        var httpClient = await _hostFixture.GetAuthenticatedHttpClientAsync(tenant.Id);

        // Act
        var response = await httpClient.GetAsync(requestUri);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<List<GetMerchantListVm>>(content, _hostFixture.SerializerOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.First().Id.Should().Be(merchant.Id);
        result.First().Title.Should().Be(merchant.Title);
    }
}