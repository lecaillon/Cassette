# Cassette [![Build Status](https://lecaillon.visualstudio.com/Cassette-CI/_apis/build/status/Cassette-CI?branchName=master)](https://lecaillon.visualstudio.com/Cassette-CI/_build/latest?definitionId=6&branchName=master) 

<img align="right" width="256px" height="256px" src="https://raw.githubusercontent.com/lecaillon/Cassette/master/images/logo256.png">

Records and replays successful HTTP responses in your testing environment.

In a micro-service environment, where your integration tests depend on a lot of external HTTP resources, Cassette is an ideal tool to improve the stability of your CI pipeline.
It is based on a very simple idea: uniquely identify all the requests that pass through and record succesfull reponses. After recording, replay the same responses without actually calling the real REST endpoint.

- Improves the stability of your testing environment.
- Avoids to many calls to HTTP servers that your application depends on, each time your CI pipeline runs.
- Speeds up the execution of your test suite.


### Installation

Cassette is available as a single [NuGet package](https://www.nuget.org/packages/Cassette.Http).

```
Install-Package Cassette.Http
```

### Usage

