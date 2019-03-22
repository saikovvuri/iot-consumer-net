<#
.SYNOPSIS
  Install the Full Infrastructure As Code Solution
.DESCRIPTION
  This Script will install all the infrastructure needed for the solution.

  1. Resource Group


.EXAMPLE
  .\install.ps1
  Version History
  v1.0   - Initial Release
#>
#Requires -Version 5.1
#Requires -Module @{ModuleName='AzureRM.Resources'; ModuleVersion='5.0'}

param(
  [string] $run = "start"
)

$Env:GROUP = [string]::Empty
$Env:REGISTRY_SERVER = [string]::Empty
$Env:HUB = [string]::Empty
$Env:EVENT_HUB_ENDPOINT = [string]::Empty
$Env:STORAGE_ACCOUNT_NAME = [string]::Empty
$Env:STORAGE_ACCOUNT_KEY = [string]::Empty
$Env:STORAGE_CONTAINER = [string]::Empty


# Load Project Environment Settings and Functions
if (Test-Path "$PSScriptRoot\.env.ps1") { . "$PSScriptRoot\.env.ps1" }

$GROUP = $Env:GROUP
$REGISTRY_SERVER = $Env:REGISTRY_SERVER
$HUB = $Env:HUB
$EVENT_HUB_ENDPOINT = $Env:EVENT_HUB_ENDPOINT
$APPINSIGHTS_INSTRUMENTATIONKEY = $Env:APPINSIGHTS_INSTRUMENTATIONKEY
$STORAGE_ACCOUNT_NAME = $Env:STORAGE_ACCOUNT_NAME
$STORAGE_ACCOUNT_KEY = $Env:STORAGE_ACCOUNT_KEY
$STORAGE_CONTAINER = $Env:STORAGE_CONTAINER


# Display Project Environment Settings
if ($run -eq "env") {
  Get-ChildItem Env:GROUP -ErrorAction SilentlyContinue
  Get-ChildItem Env:REGISTRY_SERVER -ErrorAction SilentlyContinue
  Get-ChildItem Env:HUB -ErrorAction SilentlyContinue
  Get-ChildItem Env:EVENT_HUB_ENDPOINT -ErrorAction SilentlyContinue
  Get-ChildItem Env:APPINSIGHTS_INSTRUMENTATIONKEY -ErrorAction SilentlyContinue
  Get-ChildItem Env:STORAGE_ACCOUNT_NAME -ErrorAction SilentlyContinue
  Get-ChildItem Env:STORAGE_ACCOUNT_KEY -ErrorAction SilentlyContinue
  Get-ChildItem Env:STORAGE_CONTAINER -ErrorAction SilentlyContinue
}



if ($run -eq "docker") {
  Write-Host "Building Docker...." -ForegroundColor "cyan"
  docker build -t $REGISTRY_SERVER/iot-consumer-net:latest .

  Write-Host "Gathering EventHub Endpoint...." -ForegroundColor "cyan"
  $POLICY = "iothubowner"
  $ENDPOINT = (az iot hub show -n $HUB --query properties.eventHubEndpoints.events.endpoint -otsv)
  $SHARED_ACCESS_KEY = (az iot hub policy show --hub-name $HUB --name $POLICY --query primaryKey -otsv)
  $EVENT_HUB_ENDPOINT = "Endpoint=$ENDPOINT;SharedAccessKeyName=$POLICY;SharedAccessKey=$SHARED_ACCESS_KEY;EntityPath=$HUB"
  $STORAGE_ACCOUNT_KEY = (az storage account keys list --resource-group $GROUP --account-name $STORAGE_ACCOUNT_NAME  --query '[0].value' -otsv)

  Write-Host "Starting up Docker...." -ForegroundColor "cyan"
  docker run -it --name $HUB -e HUB=$HUB -e STORAGE_ACCOUNT_NAME=$STORAGE_ACCOUNT_NAME -e STORAGE_ACCOUNT_KEY=$STORAGE_ACCOUNT_KEY -e APPINSIGHTS_INSTRUMENTATIONKEY=$APPINSIGHTS_INSTRUMENTATIONKEY -e EVENT_HUB_ENDPOINT=$EVENT_HUB_ENDPOINT  $REGISTRY_SERVER/iot-consumer-net:latest
}

if ($run -eq "docker:stop") {
  Write-Host "Shutting down Docker...." -ForegroundColor "cyan"
  docker rm -f $HUB
}

if ($run -eq "start") {
  Write-Host "Gathering EventHub Endpoint...." -ForegroundColor "cyan"
  $POLICY = "iothubowner"
  $ENDPOINT = (az iot hub show -n $HUB --query properties.eventHubEndpoints.events.endpoint -otsv)
  $SHARED_ACCESS_KEY = (az iot hub policy show --hub-name $HUB --name $POLICY --query primaryKey -otsv)

  Write-Host "Starting Event Processor Host...." -ForegroundColor "cyan"
  dotnet build
  $Env:EVENT_HUB_ENDPOINT = "Endpoint=$Endpoint;SharedAccessKeyName=$POLICY;SharedAccessKey=$SHARED_ACCESS_KEY;EntityPath=$HUB"
  $Env:STORAGE_ACCOUNT_KEY = (az storage account keys list --resource-group $GROUP --account-name $STORAGE_ACCOUNT_NAME  --query '[0].value' -otsv)
  dotnet run
}
