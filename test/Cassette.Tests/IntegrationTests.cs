using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Refit;
using Xunit;

namespace Cassette.Tests;

public class IntegrationTests
{
    private const string KeyGetRegion11Deps = "Cassette:Tests:GET:https//geo.api.gouv.fr/regions/11/departements:02j5xk5DetP1iTdH+UIGnE25214=";
    private const string KeyGetRegionWithLastSegmentExcluded = "Cassette:Tests:GET:https//geo.api.gouv.fr/regions/:qxrE8t5NkfBXyxPRiOe8hfY2dhE=";
    private const string KeyGetAllRegions = "Cassette:Tests:GET:https//geo.api.gouv.fr/regions:Oc06qZzKHX4uELWVZo46hBoCAoc=";

    // 1: NoCache
    private readonly IGeoApi _geoApi1;
    private readonly IDistributedCache _cache1;

    // 2: AddCassette()
    private readonly IGeoApi _geoApi2;
    private readonly IDistributedCache _cache2;
    private readonly CassetteOptions _options2;

    public IntegrationTests()
    {
        var builder = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddDistributedMemoryCache();
            services.AddRefitClient<IGeoApi>().ConfigureHttpClient(options => options.BaseAddress = new Uri("https://geo.api.gouv.fr"));
        })
        .Build();

        _geoApi1 = builder.Services.GetRequiredService<IGeoApi>();
        _cache1 = builder.Services.GetRequiredService<IDistributedCache>();

        var builder2 = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            // Declare the cache implementation Cassette will rely on
            services.AddDistributedMemoryCache();

            // Register Cassette in the DI container
            services.AddCassette(options =>
            {
                options.KeyPrefix = "Cassette:Tests";
                options.CacheEntryOption.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
            });

            services.AddRefitClient<IGeoApi>()
                    .ConfigureHttpClient(options => options.BaseAddress = new Uri("https://geo.api.gouv.fr"))
                    .AddReplayingHttpMessageHandler(); // Add the replaying message handler for the the IGeoApi,
                                                       // only if Cassette has been previously registered by calling AddCassette().
                                                       // The idea is to activate Cassette only during the integration tests.
        })
        .Build();

        _geoApi2 = builder2.Services.GetRequiredService<IGeoApi>();
        _cache2 = builder2.Services.GetRequiredService<IDistributedCache>();
        _options2 = builder2.Services.GetRequiredService<IOptions<CassetteOptions>>().Value;
    }

    [Fact]
    public async Task Should_Record_Http_Request_When_Cassette_Is_Activated()
    {
        var depA = await _geoApi2.GetDepartements("11");
        byte[] cassetteA = await _cache2.GetAsync(KeyGetRegion11Deps);
        cassetteA.Should().NotBeNull("The cassette should have been cached.");

        await WaitHalftTheDurationOfTheExpirationTime();

        var depB = await _geoApi2.GetDepartements("11");
        byte[] cassetteB = await _cache2.GetAsync(KeyGetRegion11Deps);
        cassetteB.Should().NotBeNull("The cassette should still be in cache, because the expiration time has not been reached.");
        depA.Should().BeEquivalentTo(depB, "The 2 departments should be the same.");
        cassetteA.Should().BeEquivalentTo(cassetteB, "The 2 cassettes should be the same.");

        await WaitHalftTheDurationOfTheExpirationTime();

        (await _cache2.GetAsync(KeyGetRegion11Deps)).Should().BeNull("The cache should be empty, because the expiration time has been reached.");
    }

    [Fact]
    public async Task Should_Exclude_Last_Segment_Of_The_Uri_From_The_Key_Cache()
    {
        var region93 = await _geoApi2.GetRegion("93");
        byte[] cassetteA = await _cache2.GetAsync(KeyGetRegionWithLastSegmentExcluded);
        cassetteA.Should().NotBeNull("The cassette should have been cached without the last segment in the key.");

        var region44 = await _geoApi2.GetRegion("44");
        byte[] cassetteB = await _cache2.GetAsync(KeyGetRegionWithLastSegmentExcluded);
        cassetteB.Should().NotBeNull("The cassette should already be in the cache due to the ExcludeLastUriSegment HTTP header.");
        region44.Should().BeEquivalentTo(region93, "What ever the given region code, the response should be region93 because [ExcludeLastUriSegment] is used.");
    }

    [Fact]
    public async Task Should_Not_Record_Anything_When_Cassette_Is_Not_Activated()
    {
        await _geoApi1.GetDepartements("11");
        (await _cache1.GetAsync(KeyGetRegion11Deps)).Should().BeNull("The cache should be empty, because Cassette is not activated.");
    }

    [Fact]
    public async Task Should_Not_Record_Anything_When_NoRecord_Is_Used()
    {
        await _geoApi2.GetRegions();
        (await _cache2.GetAsync(KeyGetAllRegions)).Should().BeNull("The cache should be empty, because [NoRecord] is used.");
    }

    private Task WaitHalftTheDurationOfTheExpirationTime() 
        => Task.Delay((int)_options2.CacheEntryOption.AbsoluteExpirationRelativeToNow.Value.TotalMilliseconds / 2);
}
