using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
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
                .AddOptions()
                .BuildServiceProvider();

            var handler = new ReplayingHandler(serviceProvider.GetService<IDistributedCache>(), serviceProvider.GetService<IOptions<CassetteOptions>>())
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
            var regions = await _geoApi.GetRegionsAsync("32");
            var regions2 = await _geoApi.GetRegionsAsync("32");

            // Assert
            regions.Should().BeEquivalentTo(regions2);
        }
    }
}