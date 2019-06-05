# Cassette [![Build Status](https://lecaillon.visualstudio.com/Cassette-CI/_apis/build/status/Cassette-CI?branchName=master)](https://lecaillon.visualstudio.com/Cassette-CI/_build/latest?definitionId=6&branchName=master) 

<img align="right" width="256px" height="256px" src="https://raw.githubusercontent.com/lecaillon/Cassette/master/images/logo256.png">

Records and replays successful HTTP responses in your testing environment.

In a micro-service context, where your integration tests depend on a lot of external HTTP resources, Cassette is an ideal tool to improve the stability of your CI pipeline.
It is based on a very simple idea: uniquely identify all the requests that pass through and record succesfull reponses. After recording, replay the same responses without actually calling the real REST endpoint.

### Key features
- Improves the stability of your testing environment.
- Avoids to many calls to HTTP servers that your application depends on, each time your CI pipeline runs.
- Speeds up the execution of your test suite.


### Installation
Cassette is available as a single [NuGet package](https://www.nuget.org/packages/Cassette.Http).

```
Install-Package Cassette.Http
```

### Usage
Most of the time Cassette will have to be configured both in your application as well as in the associated test project.

#### In the application
The easiest way to configure Cassette is to use the [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests). It will allow you to add the `ReplayingHandler` to every `HttpClient`. A sample that also uses [Refit](https://github.com/reactiveui/refit) is available [here](https://github.com/lecaillon/Cassette/blob/7b2b95c42624dc0a21b1e6968aa01a106bc35ea2/samples/AspNetCore.HttpClientFactory.QuickStart/Startup.cs#L24).
> Until the `AddCassette()` method has been called, the HTTP message handler is not really added to the HttpClient so its behavior remains the same.
```c#
services.AddRefitClient<IGeoApi>()
        .ConfigureHttpClient(options => options.BaseAddress = new Uri("https://geo.api.gouv.fr"))
        .AddReplayingHttpMessageHandler(); // Add the replaying message handler for the the IGeoApi,
                                           // only if Cassette has been previously registered by calling AddCassette().
                                           // The idea is to activate Cassette only during the integration tests.
```

#### In the test project
