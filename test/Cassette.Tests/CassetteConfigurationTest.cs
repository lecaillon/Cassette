using System;
using System.Threading.Tasks;
using Cassette.Tests.WebApplication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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

    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices((builderContext, services) =>
            {
                // Declare the cache implementation Cassette will rely on
                services.AddDistributedMemoryCache();

                // Register Cassette in a test env only
                services.AddCassette(options =>
                {
                    options.KeyPrefix = "Cassette";
                    options.CacheEntryOption.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                });
            });
        }
    }
}
