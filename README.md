# iot-consumer-net

A Simple IoT Event Processor written in dotnet with Docker Support

__PreRequisites__

Requires the use of [direnv](https://direnv.net/).  
Requires the use of [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).  
Requires the use of [Docker](https://www.docker.com/get-started).  

### Supported Use Cases

1. __Localhost Event Processor Host__

    _On a localhost consume events using an event processor host_


1. __Docker Event Processor Host__

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
# Setup the Environment Variables in .env.ps1
#----------------------------------
$Env:GROUP = "iot-resources"
$Env:HUB = (az iot hub list --resource-group $GROUP --query [].name -otsv)
$Env:STORAGE_ACCOUNT_NAME = (az storage account list --resource-group $GROUP --query [].name -otsv)
#----------------------------------

# Run the Processor
./task.ps1
```

Linux bash
```bash
# Setup the Environment Variables
GROUP="iot-resources"
export HUB=$(az iot hub list --resource-group $GROUP --query [].name -otsv)
export STORAGE_ACCOUNT_NAME=$(az storage account list --resource-group $GROUP --query [].name -otsv)
export STORAGE_ACCOUNT_KEY=$(az storage account keys list --resource-group $GROUP --account-name $Env:STORAGE_ACCOUNT_NAME  --query '[0].value' -otsv)
export EVENT_HUB_ENDPOINT="<event_hub_endpoint>"

# Run the Device
npm start
```

## Docker Container Consumer

Windows Powershell
```powershell
# Setup the Environment Variables
#----------------------------------
$GROUP = "iot-resources"
$Env:HUB = (az iot hub list --resource-group $GROUP --query [].name -otsv)
$Env:STORAGE_ACCOUNT_NAME = (az storage account list --resource-group $GROUP --query [].name -otsv)
$Env:STORAGE_ACCOUNT_KEY = (az storage account keys list --resource-group $GROUP --account-name $Env:STORAGE_ACCOUNT_NAME  --query '[0].value' -otsv)

$POLICY = "iothubowner"
$ENDPOINT = (az iot hub show -n $Env:HUB --query properties.eventHubEndpoints.events.endpoint -otsv)
$SHARED_ACCESS_KEY = (az iot hub policy show --hub-name $Env:HUB --name $POLICY --query primaryKey -otsv)
$EVENT_HUB_ENDPOINT = "Endpoint=$ENDPOINT;SharedAccessKeyName=$POLICY;SharedAccessKey=$SHARED_ACCESS_KEY;EntityPath=$HUB"

$REGISTRY_SERVER = "danielscholl"
#----------------------------------

# Run the Docker Device
./task.ps1 -run docker

# Stop and Remove the Device
./task.ps1 -run docker:stop
```

Linux bash
```bash
# Setup the Environment Variables
export GROUP="iot-resources"
export REGISTRY_SERVER="danielscholl"
export HUB=$(az iot hub list --resource-group $GROUP --query [].name -otsv)
export STORAGE_ACCOUNT_NAME=$(az storage account list --resource-group $GROUP --query [].name -otsv)
export APPINSIGHTS_INSTRUMENTATIONKEY="<your_appinsights_key>"

# Run the Docker Device
npm run docker

# Stop and Remove the Device
npm run docker:stop
```

## Azure Container Instances Consumer

```bash
COUNT=1
until [ $COUNT -gt 4 ]; do
  aci/deploy.sh eph$COUNT
  az container create --resource-group ${GROUP} --file aci/eph$COUNT.yaml -oyaml
  let COUNT+=1
done
```
