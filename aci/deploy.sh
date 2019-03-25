#!/usr/bin/env bash
#
#  Purpose: Generate ACI Deployment
#  Usage:
#    deploy.sh <type> <name>


printf "\n"
tput setaf 2; echo "Creating ACI Deployment" ; tput sgr0
tput setaf 3; echo "-----------------------" ; tput sgr0

STORAGE_ACCOUNT_NAME=$(az storage account list --resource-group $GROUP --query [].name -otsv)
STORAGE_ACCOUNT_KEY=$(az storage account keys list --resource-group $GROUP --account-name $STORAGE_ACCOUNT_NAME  --query '[0].value' -otsv)

HUB=$(az iot hub list --resource-group $GROUP --query [].name -otsv)

POLICY="iothubowner"
ENDPOINT=$(az iot hub show -n $HUB --query properties.eventHubEndpoints.events.endpoint -otsv)
SHARED_ACCESS_KEY=$(az iot hub policy show --hub-name $HUB --name $POLICY --query primaryKey -otsv)
EVENT_HUB_ENDPOINT="Endpoint=$ENDPOINT;SharedAccessKeyName=$POLICY;SharedAccessKey=$SHARED_ACCESS_KEY;EntityPath=$HUB"
APPINSIGHTS_INSTRUMENTATIONKEY=$(az resource list -g $GROUP --query "[?type=='Microsoft.Insights/components']".name -otsv)

cat > ./aci/deploy.yaml << EOF
apiVersion: '2018-06-01'
location: eastus
name: eventprocessorhost
properties:
  containers:
  - name: eventprocessorhost
    properties:
      environmentVariables:
        - name: 'STORAGE_ACCOUNT_NAME'
          value: '$STORAGE_ACCOUNT_NAME'
        - name: 'STORAGE_ACCOUNT_KEY'
          value: '$STORAGE_ACCOUNT_KEY'
        - name: 'EVENT_HUB_ENDPOINT'
          value: '$EVENT_HUB_ENDPOINT'
        - name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: '$APPINSIGHTS_INSTRUMENTATIONKEY'
      image: danielscholl/iot-consumer-net:latest
      ports: []
      resources:
        requests:
          cpu: 1.0
          memoryInGB: 1.5
  osType: Linux
  restartPolicy: Always
tags: {}
type: Microsoft.ContainerInstance/containerGroups
EOF

echo "    ./aci/deploy.yaml"

