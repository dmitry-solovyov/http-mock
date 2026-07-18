# HttpMock

[![.NET](https://github.com/dmitry-solovyov/http-mock/actions/workflows/dotnet.yml/badge.svg)](https://github.com/dmitry-solovyov/http-mock/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/HttpMock.Tool.svg)](https://www.nuget.org/packages/HttpMock.Tool)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

HttpMock replaces external HTTP services with a configurable mocked instance for quick testing scenarios.

- A set of endpoints is configured through an HTTP request; the mocked endpoint is then used instead of the real one.
- Point the code under test at the mock server's URL instead of the real web service.
- Each endpoint can mock the response body, status code, processing delay and custom headers.
- The mock server runs as a .NET global tool from the command line.

## Table of contents

- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Installation](#installation)
  - [Option A: Install from NuGet](#option-a-install-from-nuget)
  - [Option B: Build and install from source](#option-b-build-and-install-from-source)
  - [Uninstall](#uninstall)
- [Running the tool](#running-the-tool)
- [Configuring the running application](#configuring-the-running-application)
  - [Configuration schema](#configuration-schema)
  - [Example configuration](#example-configuration)
  - [Set the configuration (PUT)](#set-the-configuration-put)
  - [Review the configuration (GET)](#review-the-configuration-get)
  - [Test a mocked endpoint](#test-a-mocked-endpoint)
- [License](#license)

## Prerequisites

The application targets .NET 10.0. The .NET SDK must be installed on the machine.

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

| Parameter | Default | Description |
|-----------|---------|-------------|
| `port`    | —       | Port the mock server listens on. |
| `quiet`   | `0`     | `0` = log output enabled, `1` = suppress logs. |

## Configuring the running application

Once running, mocked endpoints are available at `http://0.0.0.0/{mocked-endpoint-path}`.

### Configuration schema

| Field         | Type     | Default        | Description |
|---------------|----------|----------------|-------------|
| `Path`        | string   | —              | Relative path of the mocked endpoint (required). |
| `Method`      | string   | `get`          | HTTP method to match. |
| `Status`      | int      | `200`          | HTTP status code returned. |
| `Delay`       | int (ms) | `0` (max `60000`) | Artificial processing delay before responding. |
| `Description` | string   | `""`           | Free-text note describing the endpoint. |
| `Payload`     | string   | `""`           | Response body. |
| `Headers`     | map      | `{}`           | Custom response headers. |

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
      'X-ServerHeader': /Example/Redirect

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
