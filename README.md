# http-mock

## Configuration request

### Schema of the configuration request:

```
Info:
Map[]
  ┝ Url
  ┝ Method
  ┝ Status
  ┝ Delay
  ┝ Description
  └ Payload
```

### Example of the configuration request:

```yaml
Info: Example requests
Map:
  - Url: /probe
    Description: successful probe action
    Method: get
    Status: 200
    Delay: 200

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

## Install the tool

```powershell
dotnet tool install -g httpservermock.tool --add-source ./nupkg
```

## Run the tool

```powershell
httpservermock --server * --port 8888 --schema http
```