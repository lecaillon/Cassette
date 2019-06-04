using System.Threading.Tasks;
using AspNetCore.HttpClientFactory.QuickStart;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Cassette.Tests
{
    public class CassetteConfigurationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public CassetteConfigurationTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/geo/regions");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
