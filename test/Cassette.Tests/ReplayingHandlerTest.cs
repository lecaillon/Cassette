using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cassette.Tests.WebApplication;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using Xunit;

namespace Cassette.Tests
{
    public class ReplayingHandlerTest
    {
        readonly IGeoApi _geoApi;

        public ReplayingHandlerTest()
        {
            var serviceProvider = new ServiceCollection()
                .AddDistributedMemoryCache()
                .AddCassette(options =>
                {
                    options.KeyPrefix = "Cassette";
                    options.CacheEntryOption.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                })
                .BuildServiceProvider();

            var handler = new ReplayingHandler(
                cache: serviceProvider.GetService<IDistributedCache>(),
                options: serviceProvider.GetService<IOptions<CassetteOptions>>(),
                logger: serviceProvider.GetService<ILogger<ReplayingHandler>>())
            {
                InnerHandler = new HttpClientHandler()
            };

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://geo.api.gouv.fr")
            };

            _geoApi = RestService.For<IGeoApi>(httpClient);
        }

        [Fact]
        public async Task Should_Same_Requests_Return_Same_Responses()
        {
            // Act
            var regions = await _geoApi.GetRegions();
            var regions2 = await _geoApi.GetRegions();

            // Assert
            regions.Should().BeEquivalentTo(regions2);
        }
    }
}