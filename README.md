# Cassette [![Build Status](https://lecaillon.visualstudio.com/Cassette-CI/_apis/build/status/Cassette-CI?branchName=master)](https://lecaillon.visualstudio.com/Cassette-CI/_build/latest?definitionId=6&branchName=master) 

<img align="right" width="256px" height="256px" src="https://raw.githubusercontent.com/lecaillon/Cassette/master/images/logo256.png">

Records and replays successful HTTP responses in your testing environment.

In a micro-service context, where your integration tests depend on a lot of external HTTP resources, Cassette is an ideal tool to improve the stability of your CI pipeline.
It is based on a very simple idea: uniquely identify all the requests that pass through and record succesfull reponses. After recording, replay the same responses without actually calling the real REST endpoint.

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
        .AddReplayingHttpMessageHandler(); // Add the replaying message handler for the the IGeoApi,
                                           // only if Cassette has been previously registered by calling AddCassette().
                                           // The idea is to activate Cassette only during the integration tests.
```

> Until the `AddCassette()` configuration method has been called, the HTTP message handler is not really added to the HttpClient, so its behavior remains unchanged.

2. Finally activate Cassette in integration tests, by calling the `AddCassette()` configuration method **before** the registration of the message handlers. Either by 
