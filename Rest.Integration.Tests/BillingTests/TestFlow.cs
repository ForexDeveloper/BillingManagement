using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.TenantAggregate;
using FluentAssertions;
using IntegrationTest.Server;
using Microsoft.OpenApi.Validations;
using Newtonsoft.Json.Linq;
using Rest.Integration.Tests.Base;
using Rest.Integration.Tests.Models;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rest.Integration.Tests.PaymentTests
{

    [Collection(nameof(SharedHostCollection))]
    public class TestFlow
    {
        private readonly SharedHostFixture _hostFixture;

        public TestFlow(SharedHostFixture hostFixture)
        {
            _hostFixture = hostFixture;
        }

        [Fact]
        public async Task GetList_ShouldReturnListItems()
        {
            // Arrange
            var tenant = _hostFixture.Tenant;
            
            var httpClient = await _hostFixture.GetAuthenticatedHttpClientAsync();
            const string requestUri = "api/tenant-panel/merchants";
          

            // Act
            //var response = await httpClient.GetAsync(requestUri);
            //var content = await response.Content.ReadAsStringAsync();
            //var result = JsonSerializer.Deserialize<CpgPaymentGetTokenResponseV2>(content, _hostFixture.SerializerOptions);

            // Assert
            //response.StatusCode.Should().Be(HttpStatusCode.OK);
            //result.Should().NotBeNull();
            //result.Token.Should().NotBeEmpty();
            
        }



      
    }
}
