param appName string
param appServicePlanName string
param location string
param environmentVariables array
param resourceTags object
param sku object

resource appServicePlan 'Microsoft.Web/serverFarms@2020-12-01' = {
  name: appServicePlanName
  location: location
  tags: resourceTags
  kind: 'app'
  sku: sku
}

resource appService 'Microsoft.Web/sites@2020-12-01' = {
  name: appName
  location: location
  tags: resourceTags
  kind: 'app'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      ftpsState: 'Disabled'
      use32BitWorkerProcess: true
      http20Enabled: true
      minTlsVersion: '1.2'
      appSettings: environmentVariables
      netFrameworkVersion: 'v6.0'
    }
  }
}

output principalId string = appService.identity.principalId
output baseUrl string = appService.properties.defaultHostName
