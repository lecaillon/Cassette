using System.Threading.Tasks;
using AspNetCore.HttpClientFactory.QuickStart;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using Xunit;

namespace Cassette.Tests
{
    public class ReplayingHandlerTest : IClassFixture<WebApplicationFactory<Startup>>, IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private const string KeyGetRegion01 = "Cassette:Tests:GET:https//geo.api.gouv.fr/regions/01:aK+5NU909Wdt18bjp6NFuml5dIo=";

        // 1: NoCache
        private readonly IGeoApi _geoApi1;
        private readonly IDistributedCache _cache1;

        // 2: AddCassette()
        private readonly IGeoApi _geoApi2;
        private readonly IDistributedCache _cache2;
        private readonly CassetteOptions _options2;

        public ReplayingHandlerTest(WebApplicationFactory<Startup> factory1, CustomWebApplicationFactory<Startup> factory2)
        {
            _geoApi1 = RestService.For<IGeoApi>(factory1.CreateClient());
            _cache1 = factory1.Server.Host.Services.GetRequiredService<IDistributedCache>();

            _geoApi2 = RestService.For<IGeoApi>(factory2.CreateClient());
            _cache2 = factory2.Server.Host.Services.GetRequiredService<IDistributedCache>();
            _options2 = factory2.Server.Host.Services.GetRequiredService<IOptions<CassetteOptions>>().Value;
        }

        [Fact]
        public async Task Should_Record_Http_Request_When_Cassette_Is_Activated()
        {
            var regionA = await _geoApi2.GetRegion("01");
            byte[] cassetteA = await _cache2.GetAsync(KeyGetRegion01);
            cassetteA.Should().NotBeNull("The cassette should have been cached.");

            await WaitHalftTheDurationOfTheExpirationTime();

            var regionB = await _geoApi2.GetRegion("01");
            byte[] cassetteB = await _cache2.GetAsync(KeyGetRegion01);
            cassetteB.Should().NotBeNull("The cassette should still be in cache, because the expiration time has not been reached.");
            regionA.Should().BeEquivalentTo(regionB, "The 2 regions should be the same.");
            cassetteA.Should().BeEquivalentTo(cassetteB, "The 2 cassettes should be the same.");

            await WaitHalftTheDurationOfTheExpirationTime();

            (await _cache2.GetAsync(KeyGetRegion01)).Should().BeNull("The cache should be empty, because the expiration time has been reached.");
        }

        [Fact]
        public async Task Should_Not_Record_Anything_When_Cassette_Is_Not_Activated()
        {
            await _geoApi1.GetRegion("01");
            (await _cache1.GetAsync(KeyGetRegion01)).Should().BeNull("The cache should be empty, because Cassette is not activated.");
        }

        private Task WaitHalftTheDurationOfTheExpirationTime() 
            => Task.Delay((int)_options2.CacheEntryOption.AbsoluteExpirationRelativeToNow.Value.TotalMilliseconds / 2);
    }
}
