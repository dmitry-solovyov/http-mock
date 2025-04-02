# HttpMock

The application helps to replace an external service with a mocked instance for quick testing scenarios.
A set of the endpoints can be configured through the HTTP request. Th mocked endpoint can be used instead of the real one.
The address of the real web service should be replaced with the URL of the mock service.
The configuration allows the response body, status code and processing delay to be mocked. 
The response can also be provided with a custom header.


The mock server can be started as a dotnet tool from the command line or from the Docker container.

## Local installation of the application as a tool

### Pack the tool

Working folder: `\src\HttpMock.Tool`

```powershell
dotnet pack --output nupkg -p:TargetFrameworks=net8.0 --runtime win-x64 --configuration Release
```

The resulted package will be placed in the `/nupkg` directory.

### Install the tool

```powershell
dotnet tool install -g httpmock.tool --add-source ./nupkg
```

Install the application as a global tool (`-g` parameter).

or command line for a specific version `1.0.0`:
```powershell
dotnet tool install -g httpmock.tool --add-source ./nupkg --version 1.0.0
```

### Uninstall the tool

```
dotnet tool uninstall -g httpmock.tool
```

### Run the application as a tool

```powershell
httpMock port=58888 quiet=0
```
Binds the tool to port 58888. Quiet mode (parameter value `1`) allows the tool to run without outputting logs.

## Configuring running application

The mocked endpoint should be available by the relative address specified in the configuration: `http://0.0.0.0/{mocked-endpoint-path}`.

### Schema of the configuration request:

```
Endpoints[]
  ┝ Path
  ┝ Method
  ┝ Status
  ┝ Delay
  ┝ Description
  └ Payload
```

### Example of the configuration:

```yaml
Endpoints:
  - Path: /probe
    Description: successful probe action
    Status: 200
    Delay: 50

  - Path: /probe
    Description: failer probe action
    Method: get
    Status: 502
    Delay: 2000

  - Path: /probe/detailed
    Method: post
    Status: 200
    Payload: '{"success":"true"}'
    Headers:
      'X-ServerHeader': /Example/Redirect
```

### Configuration setup request (PUT method)

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

Invoke-RestMethod @request
```
Setup the configuration of the mocked endpoints.

### Review configuration request (GET method)

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

Invoke-RestMethod @request
```
Review the configuration of the mocked endpoints.

### Test the mocked endpoint

```Powershell
$request = @{
    Method = 'GET'
    Uri    = "http://localhost:58888/probe"
}

Invoke-RestMethod @request
```
Invoke `/probe` request configured before.