# Cassette [![Build Status](https://lecaillon.visualstudio.com/Cassette-CI/_apis/build/status/Cassette-CI?branchName=master)](https://lecaillon.visualstudio.com/Cassette-CI/_build/latest?definitionId=6&branchName=master) [![Coverage status](https://img.shields.io/azure-devops/coverage/lecaillon/cassette-ci/6.svg?color=brightgreen)](https://lecaillon.visualstudio.com/Cassette-CI/_build/latest?definitionId=6&branchName=master)

<img align="right" width="256px" height="256px" src="https://raw.githubusercontent.com/lecaillon/Cassette/master/images/logo256.png">

Records and replays successful HTTP responses in your testing environment.

In a micro-service context, where your integration tests depend on a lot of external HTTP resources, Cassette is an ideal tool to improve the stability of your CI pipeline.
It is based on a very simple idea: uniquely identify all the requests that pass through and record succesfull reponses. After recording, replay the same responses without actually calling the real REST endpoint.

To create that unique request identifier, Cassette computes an hash from the HTTP method, the uri and the body.

### Key features
- Improves the stability of your testing environment.
- Speeds up the execution of your test suite.
- Avoids to many calls to your HTTP API dependencies, each time your CI pipeline runs.

### Installation
Cassette is available as a single [NuGet package](https://www.nuget.org/packages/Cassette.Http).

```
Install-Package Cassette.Http
```

### Requirement
- **.NET Core 2+** or **.NET 4.6.1+**
- Register an implementation of [`IDistributedCache`](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed) (Redis, SQL Server, in-memory).

### Usage
1. The easiest way to configure Cassette is to use the [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests). It will allow you to add the `ReplayingHandler` to every `HttpClient`. A sample that also uses [Refit](https://github.com/reactiveui/refit) is available [here](https://github.com/lecaillon/Cassette/blob/master/samples/AspNetCore.HttpClientFactory.QuickStart/Startup.cs).

```c#
services.AddRefitClient<IGeoApi>()
        .ConfigureHttpClient(options => options.BaseAddress = new Uri("https://geo.api.gouv.fr"))
        .AddReplayingHttpMessageHandler(); // Add the replaying message handler for the the IGeoApi, only
                                           // if Cassette has been previously registered by calling AddCassette().
                                           // The idea is to activate Cassette only during the integration tests.
```

> Until the `AddCassette()` configuration method has been called, the HTTP message handler is not really added to the HttpClient, so its behavior remains unchanged.

2. Finally activate Cassette during integration tests, by calling the `AddCassette()` configuration method **before** the registration of the message handlers. You can either use a feature toggle or even better create an [integration test project](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests) to override your DI configuration. Another sample can be found [here](https://github.com/lecaillon/Cassette/blob/master/test/Cassette.Tests/CustomWebApplicationFactory.cs).

```c#
// Declare the cache implementation Cassette will rely on
services.AddDistributedMemoryCache();

// Register Cassette in the DI container
services.AddCassette(options =>
{
    options.KeyPrefix = "Cassette";
    options.CacheEntryOption.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
});
```

### Configuration
Cassette uses specific HTTP request headers to modify its behavior. They are defined in the classes `CassetteOptions` and `CassetteOptions.Refit`:
- **NoRecord**: prevent caching of the HTTP response.
- **ExcludeRequestBody**: exclude the request body from the computed key in cache when it contains an auto-generated identifier that would cause cache misses.
- **ExcludeLastUriSegment**: exclude from the computed key in cache the last uri segment when its value is always different between calls, causing cache misses.

#### Example of a Refit API where the POST endpoint is skipped by Cassette.
```c#
public interface IGeoApi
{
    [Get("/regions")]
    Task<List<Region>> GetRegions();

    [Headers(CassetteOptions.Refit.NoRecord)]
    [Post("/regions")]
    Task CreateRegion(Region region);
}
```
