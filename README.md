# iot-consumer-net

The purpose of this solution is to consume data from the IoT Hub using an Event Processor Host Pattern


__PreRequisites__

Requires the use of [direnv](https://direnv.net/).  
Requires the use of [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).  
Requires the use of [Docker](https://www.docker.com/get-started).  

### Related Repositories

- [iot-resources](https://github.com/danielscholl/iot-resources)  - Deploying IoT Resources and x509 Management
- [iot-device-edge](https://github.com/danielscholl/iot-device-edge) - Simple Edge Testing
- [iot-device-js](https://github.com/danielscholl/iot-device-js) - Simple Device Testing (NodeJS)
- [iot-device-js](https://github.com/danielscholl/iot-device-net) - Simple Device Testing (C#)
- [iot-control-js](https://github.com/danielscholl/iot-control-js) - Simple Control Testing


### Supported Use Cases

1. __Localhost Consumer

    _On a localhost consume events using an event processor host_


1. __Docker Consumer__

    _Within a container consume events using an event processor host_


### Environment Settings

Environment Settings act differently depending upon the Operating System and the IDE that is being used.


_MacOSX_
The best method to work with environment settings here is for command line to put environment settings in a .envrc file.  The `package.json` is being used as a convenient task runner.

.envrc file format
```bash
export HUB="<iot_hub>"
export STORAGE_ACCOUNT_NAME="<storage_account>"
export STORAGE_ACCOUNT_KEY="storage_account_key>"
export STORAGE_CONTAINER="eph"
export APPINSIGHTS_INSTRUMENTATIONKEY="<app_insights_key>"
export EVENT_HUB_ENDPOINT="<event_hub_endpoint>"
```

_Windows_
The best method to work with environment settings here for command line is to put environment settings in a .env.ps1 file.  The `task.ps1` script is being used as a convenient task runner.

.env.ps1 file format
```bash
$Env:GROUP = "<resource_group>"
$Env:HUB = "<hub_name>"
$Env:REGISTRY_SERVER = "<docker_registry>"
$Env:STORAGE_ACCOUNT_NAME="<storage_account_name>"
$Env:APPINSIGHTS_INSTRUMENTATIONKEY = "<app_insights_key>"
```

_VSCode_
The best method to work with environment settings here for command line is to put environment settings in a .env file and reference it with the vscode `launch.json` file.

The device connection string must be put in the .env file as the task runner is not sending the connection string at runtime..

.env file format
```bash
HUB="<iot_hub>"
STORAGE_ACCOUNT_NAME="<storage_account>"
STORAGE_ACCOUNT_KEY="storage_account_key>"
STORAGE_CONTAINER="eph"
APPINSIGHTS_INSTRUMENTATIONKEY="<app_insights_key>"
EVENT_HUB_ENDPOINT="<event_hub_endpoint>"
```


If using Visual Studio then the environment variables are pulled out of the `Properties/launchSettings.json`

## LocalHost Consumer


Windows Powershell
```powershell

```

## Docker Container Consumer

Windows Powershell
```powershell

```
