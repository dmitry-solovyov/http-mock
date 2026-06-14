# HttpMock

HttpMock replaces external HTTP services with a configurable mocked instance for quick testing scenarios.
A set of the endpoints can be configured through the HTTP request. The mocked endpoint can be used instead of the real one.
The address of the real web service should be replaced with the URL of the mock service.
The configuration allows the response body, status code and processing delay to be mocked.
The response can also be provided with a custom header.
The mock server can be started as a dotnet tool from the command line.

# Local installation of the application as a tool

## Prerequisites:

The application is built on .NET 10.0. The .NET SDK should be installed on the machine.

## Install from NuGet

```powershell
dotnet tool install -g HttpMock.Tool
```

## Package the application as a tool

### Build the package

Working folder: `\src\HttpMock.Tool`

```powershell
dotnet pack --output nupkg -p:TargetFrameworks=net10.0 --runtime win-x64 --configuration Release
```

The resulted package will be placed in the `/nupkg` directory.

### Install the tool

```powershell
dotnet tool install -g httpmock.tool --add-source ./nupkg
```

Install the application as a global tool (`-g` parameter).

or command line for a specific version `2.0.0`:

```powershell
dotnet tool install -g httpmock.tool --add-source ./nupkg --version 2.0.0
```

## Uninstall the tool

```
dotnet tool uninstall -g httpmock.tool
```

## Run the application as a tool

Binds the tool to port 58888. Quiet mode (parameter value `1`) allows the tool to run without outputting logs.

```powershell
httpMock port=58888 quiet=0
```

## Configuring running application

The mocked endpoint should be available by the relative address specified in the configuration: `http://0.0.0.0/{mocked-endpoint-path}`.

### Schema of the configuration payload:

```
Endpoints[]
  ┝ Path
  ┝ Method
  ┝ Status
  ┝ Delay
  ┝ Description
  ┝ Payload
  └ Headers
```

Example of the configuration request body:

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

Default values for the configuration parameters:

- `Status` - 200
- `Delay` - 0 seconds (max 60000)
- `Description` - empty string
- `Payload` - empty string
- `Headers` - empty collection
- `Method` - get

### Configuration setup request (PUT method)

Setup the configuration of the mocked endpoints:

```Powershell
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

### Review configuration request (GET method)

Review the configuration of the mocked endpoints:

```Powershell
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

### Test the mocked endpoint

Invoke `/probe` request configured before:

```Powershell
$request = @{
    Method = 'GET'
    Uri    = "http://localhost:58888/probe"
}

$Response = Invoke-RestMethod @request
$Response
```
