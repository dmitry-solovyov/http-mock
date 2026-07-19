# HttpMock

[![NuGet: HttpMock.Tool](https://img.shields.io/nuget/v/HttpMock.Tool.svg?label=HttpMock.Tool)](https://www.nuget.org/packages/HttpMock.Tool)
[![NuGet: HttpMock.Server](https://img.shields.io/nuget/v/HttpMock.Server.svg?label=HttpMock.Server)](https://www.nuget.org/packages/HttpMock.Server)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

HttpMock lets you substitute external HTTP services with a configurable mock, for quick manual testing scenarios or integration tests. It can run as a standalone command-line service, or be embedded directly in your code.

- A set of endpoints is configured through an HTTP request using the YAML format; the mocked endpoint then can be used instead of the real one.
- Point the code under test at the mock server's URL instead of the real web service.
- Each endpoint can mock the response body, status code, processing delay and custom headers.
- Run it as a .NET global tool (`HttpMock.Tool`) from the command line, as a Docker container, **or** host it in-process as a library (`HttpMock.Server`) — e.g. started and stopped from a test fixture.

## Table of contents

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Installation](#installation)
  - [Option A: Install from NuGet](#option-a-install-from-nuget)
  - [Option B: Build and install from source](#option-b-build-and-install-from-source)
  - [Uninstall](#uninstall)
- [Running the tool](#running-the-tool)
- [Running with Docker](#running-with-docker)
- [Use as an in-process library](#use-as-an-in-process-library)
- [Configuring the running application](#configuring-the-running-application)
  - [Configuration schema](#configuration-schema)
  - [Path and query variables](#path-and-query-variables)
  - [Multiple endpoints with the same path and method](#multiple-endpoints-with-the-same-path-and-method)
  - [Example configuration](#example-configuration)
  - [Set the configuration (PUT)](#set-the-configuration-put)
  - [Review the configuration (GET)](#review-the-configuration-get)
  - [Test a mocked endpoint](#test-a-mocked-endpoint)
- [License](#license)

## Prerequisites

The application targets .NET 8.0 and .NET 10.0. A matching .NET SDK (or runtime, for the CLI tool) must be installed on the machine. The `HttpMock.Server` library additionally requires the **ASP.NET Core** shared runtime (not just the base .NET runtime), since it hosts an embedded Kestrel web server.

## Quick start

```powershell
# 1. Install the tool
dotnet tool install -g HttpMock.Tool

# 2. Run it on port 58888
httpMock port=58888 quiet=0

# 3. In another terminal, configure a /probe endpoint
$headers = @{ "X-HttpMock-Command" = "configurations" }
$body = "Endpoints:
  - Path: /probe
    Method: get
    Status: 200
    Payload: '{\"success\":true}'"

Invoke-RestMethod -Method PUT -Uri "http://localhost:58888" -Headers $headers -Body $body -ContentType 'application/yaml'

# 4. Call it
Invoke-RestMethod -Method GET -Uri "http://localhost:58888/probe"
```

## Installation

### Option A: Install from NuGet

```powershell
dotnet tool install -g HttpMock.Tool
```

### Option B: Build and install from source

Working folder: `\src\HttpMock.Tool`

Build the package:

```powershell
dotnet pack --output nupkg -p:TargetFrameworks=net10.0 --runtime win-x64 --configuration Release
```

The resulting package is placed in the `/nupkg` directory.

Install it as a global tool (`-g`):

```powershell
dotnet tool install -g httpmock.tool --add-source ./nupkg
```

Or install a specific version, e.g. `2.0.0`:

```powershell
dotnet tool install -g httpmock.tool --add-source ./nupkg --version 2.0.0
```

### Uninstall

```powershell
dotnet tool uninstall -g httpmock.tool
```

## Running the tool

```powershell
httpMock port=58888 quiet=0
```

| Parameter | Default | Description                                    |
| --------- | ------- | ---------------------------------------------- |
| `port`    | —       | Port the mock server listens on.               |
| `quiet`   | `0`     | `0` = log output enabled, `1` = suppress logs. |

## Running with Docker

A [Dockerfile](src/HttpMock/Dockerfile) is provided, exposing port `58888` by default. Build and run it from the repository root:

```powershell
docker build -t httpmock -f src/HttpMock/Dockerfile src/HttpMock
docker run --rm -p 58888:58888 httpmock
```

## Use as an in-process library

Instead of running `HttpMock.Tool` as a separate process, the mock server can be started and stopped in-process — useful for spinning it up per test fixture without managing an external process.

Install the `HttpMock.Server` package:

```powershell
dotnet add package HttpMock.Server
```

Start and stop it programmatically:

```csharp
using HttpMock;

var app = Application.CreateWebApplication(new StartupArguments(Port: 58888, IsQuiet: true, IsHelpRequested: false));
await app.StartAsync();

// ... configure endpoints and exercise the code under test,
// using the same HTTP-based configuration API described in
// "Configuring the running application" below ...

await app.StopAsync();
```

`Application.CreateWebApplication` returns a standard `WebApplication`, so it can also be `await`ed with `RunAsync()` if you want it to run until cancelled, or wired into a test fixture's `IAsyncLifetime`/`InitializeAsync`/`DisposeAsync`.

## Configuring the running application

The same server instance and port handle both configuration requests and mocked requests — there is no separate admin port. Which type of request is being made is determined by the presence of the `X-HttpMock-Command` header:

- **Configuration request**: include the `X-HttpMock-Command: configurations` header. Use `PUT` to replace the current set of mocked endpoints, or `GET` to read back the current configuration. The request/response body is a **YAML** document (`ContentType: application/yaml`) describing the list of endpoints, as defined in the [configuration schema](#configuration-schema) below.
- **Mocked request**: any request without the `X-HttpMock-Command` header is matched against the configured endpoints by `Path` and `Method`, and answered with the configured `Status`, `Payload`, `Delay` and `Headers`. A request that doesn't match any configured endpoint receives `404 Not Found`.

Once configured, mocked endpoints are available at `http://0.0.0.0/{mocked-endpoint-path}`.

### Configuration schema

| Field         | Type     | Default           | Description                                      |
| ------------- | -------- | ----------------- | ------------------------------------------------ |
| `Path`        | string   | —                 | Relative path of the mocked endpoint (required). |
| `Method`      | string   | `get`             | HTTP method to match.                            |
| `Status`      | int      | `200`             | HTTP status code returned.                       |
| `Delay`       | int (ms) | `0` (max `60000`) | Artificial processing delay before responding.   |
| `Description` | string   | `""`              | Free-text note describing the endpoint.          |
| `Payload`     | string   | `""`              | Response body.                                   |
| `Headers`     | map      | `{}`              | Custom response headers.                         |

### Path and query variables

A `Path` segment or query-parameter value that starts with `@` (e.g. `/test?id=@id`) is treated as a **wildcard variable**: it matches any value the incoming request provides for that segment/parameter, instead of requiring an exact match.

Captured variables can be substituted back into `Payload` by referencing the same `@name` token there, e.g. `Payload: '{"testId":"@id"}'` is replaced with the actual value received for `@id` on that request. Additionally, the special token `@guid` in `Payload` is replaced with a newly generated GUID on every response, regardless of whether it appears in the `Path`.

### Multiple endpoints with the same path and method

If more than one configured endpoint matches an incoming request (same `Path` and `Method`), HttpMock cycles through them: it picks the matching endpoint that has been served the fewest times so far, then increments its counter. In practice, with two duplicate endpoints this means requests alternate between them in the order they were configured, which is useful for scripting a sequence of responses (e.g. simulate a retry that succeeds on the second attempt).

### Example configuration

```yaml
Endpoints:
  - Path: /test?id=@id
    Method: get
    Status: 201
    Delay: 1000
    Description: successful probe action
    Payload: '{"testId":"@id","uuid":"@guid"}'
    Headers:
      "X-ServerHeader": /Example/Redirect

  - Path: /test?id=@id
    Method: get
    Status: 202
    Description: successful probe action
    Payload: '{"testId":"@id"}'

  - Path: /probe
    Method: get
    Status: 200
    Description: successful probe action
    Payload: '{"success":true}'
```

### Set the configuration (PUT)

```powershell
$headers = @{
    "X-HttpMock-Command" = "configurations"
}

$body = "Endpoints:
  - Path: /probe
    Description: successful probe action
    Method: get
    Status: 200
    Delay: 50"

$request = @{
    Method      = 'PUT'
    Uri         = "http://localhost:58888"
    headers     = $headers
    Body        = $body
    ContentType = 'application/yaml'
}

$Response = Invoke-RestMethod @request
$Response
```

An invalid configuration payload returns `400 Bad Request`.

### Review the configuration (GET)

```powershell
$headers = @{
    "X-HttpMock-Command" = "configurations"
}

$request = @{
    Method      = 'GET'
    Uri         = "http://localhost:58888"
    headers     = $headers
    ContentType = 'application/yaml'
}

$Response = Invoke-RestMethod @request
$Response
```

### Test a mocked endpoint

Invoke the `/probe` endpoint configured above:

```powershell
$request = @{
    Method = 'GET'
    Uri    = "http://localhost:58888/probe"
}

$Response = Invoke-RestMethod @request
$Response
```

## License

Licensed under the [MIT License](https://opensource.org/licenses/MIT).
