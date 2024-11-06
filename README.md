# HttpMock

The application helps to replace an external service with a mocked instance for quick testing scenarios.
The mock server can be started as a dotnet tool from the command line or from the Docker container. 
Once started, the mock server can be provided with a list of mocked endpoints.
The address of the real web service should be replaced with the URL of the mock service.
The configuration allows the response body, status code and processing delay to be mocked. 
The response can also be provided with a custom header.

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

or command line for a specific version `1.0.1`:
```powershell
dotnet tool install -g httpmock.tool --add-source ./nupkg --version 1.0.1
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

The application can run multiple configuration instances called `domains`. The domain allows the configuration settings to be managed separately.
The mocked endpoint should be present in the endpoint URL. 
The domain should be specified in the address of the mocked service: `http://0.0.0.0/{domain-name}/`.
The mocked endpoints should be relative paths.

### Schema of the configuration request:

```
Endpoints[]
  ┝ Url
  ┝ Method
  ┝ Status
  ┝ Delay
  ┝ Description
  └ Payload
```

### Example of the configuration:

```yaml
Endpoints:
  - Url: /probe
    Description: successful probe action
    Method: get
    Status: 200
    Delay: 50

  - Url: /probe
    Description: failer probe action
    Method: get
    Status: 502
    Delay: 2000

  - url: /probe/detailed
    Method: post
    Status: 200
    Payload: '{"success":"true"}'
    Headers:
      'X-ServerHeader': /Example/Redirect
```

### Configuration setup request (`set-configuration` command)

```Powershell
$headers = @{
    "X-HttpMock-Command" = "set-configuration"
    "X-HttpMock-Domain"  = "test-domain"
}

$body = "Endpoints:
  - Url: /probe
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
Setup the configuration for the `test-domain` domain.

### Review configuration request (`get-configuration` command)

```Powershell
$headers = @{
    "X-HttpMock-Command" = "get-configuration"
    "X-HttpMock-Domain"  = "test-domain"
}
$request = @{
    Method      = 'GET'
    Uri         = "http://localhost:58888"
    headers     = $headers
    ContentType = 'application/yaml'
}

Invoke-RestMethod @request
```
Review the configuration for the `test-domain` domain.

### Configuration setup request

Example of the mocked request:

```Powershell
$request = @{
    Method = 'GET'
    Uri    = "http://localhost:58888/test-domain/probe"
}

Invoke-RestMethod @request
```
Invoke `/probe` request configured for the `test-domain` domain.