targetScope = 'subscription'

param environment string = 'dev'
param applicationName string = 'bookings'
@allowed([
  'Development'
  'Staging'
  'Production'
])
param appEnvironment string = 'Development'
param location string = 'westeurope'
param dbSku object = {
  name: 'Basic'
  tier: 'Basic'
}
param appSku object = {
  tier: 'Free'
  name: 'F1'
}
param domain string = 'microsoft.onmicrosoft.com'
param callbackPath string = '/signin-oidc'

param dbAdminLogin string = 'dbUser'
@secure()
param dbAdminPassword string
param fromEmail string = 'stemih11@gmail.com'
param downstreamScopes string = 'api://43b0f80e-2c9c-4e0f-8bbc-5f0cf200110f/BookingScope.Read api://43b0f80e-2c9c-4e0f-8bbc-5f0cf200110f/BookingScope.Write'
param appClientId string = '43b0f80e-2c9c-4e0f-8bbc-5f0cf200110f'
@secure()
param appSecret string
@secure()
param sendGridApiKey string
param alwaysOn bool = false

var defaultTags = {
  'environment': environment
  'application': applicationName
}
var resourceGroupName = 'rg-${applicationName}-${environment}'
var kvName = 'kw-${applicationName}-${environment}'

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
  tags: defaultTags
}

module blobStorage 'modules/storage-blob/storage.bicep' = {
  name: 'storage'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    applicationName: applicationName
    environment: environment
    resourceTags: defaultTags
  }
}

module database 'modules/sql-server/sql-azure.bicep' = {
  name: 'sqlDb'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    applicationName: applicationName
    environment: environment
    tags: defaultTags
    administratorLogin: dbAdminLogin
    administratorPassword: dbAdminPassword
    sku: dbSku
  }
}

module redis 'modules/redis/redis.bicep' = {
  name: 'redis'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    applicationName: applicationName
    environment: environment
    resourceTags: defaultTags
  }
}

var applicationEnvironmentVariables = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: appEnvironment
  }
  {
    name: 'VAULT_URI'
    value: 'https://${kvName}${az.environment().suffixes.keyvaultDns}'
  }
]

module webApi 'modules/app-service/app-service.bicep' = {
  name: 'webApi'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    applicationName: '${applicationName}-api'
    environment: environment
    resourceTags: defaultTags
    environmentVariables: applicationEnvironmentVariables
    sku: appSku
    alwaysOn: alwaysOn
  }
}

module gui 'modules/app-service/app-service.bicep' = {
  name: 'gui'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    applicationName: '${applicationName}-gui'
    environment: environment
    resourceTags: defaultTags
    environmentVariables: applicationEnvironmentVariables
    sku: appSku
    alwaysOn: alwaysOn
  }
}


module ai 'modules/application-insights/app-insights.bicep' = {
  name: 'instrumentation'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    applicationName: applicationName
    environment: environment
    resourceTags: defaultTags
  }
}


module keyVault 'modules/key-vault/key-vault.bicep' = {
  name: 'keyVault'
  scope: resourceGroup(rg.name)
  dependsOn: [
    webApi
    ai
    redis
    database
    gui
    blobStorage
  ]
  params: {
    kvName: kvName
    resourceTags: defaultTags
    apiPrincipalId: webApi.outputs.principalId
    guiPrincipalId: gui.outputs.principalId
    location: location
    storageName: blobStorage.outputs.storageAccountName
    callbackPath: callbackPath
    clientId: appClientId
    domain: domain
    fromEmail: fromEmail
    downstreamScopes: downstreamScopes
    downstreamBaseUrl: webApi.outputs.baseUrl
    guiUrl: gui.outputs.baseUrl
    secret: appSecret
    sendGridApiKey: sendGridApiKey
    redisName: redis.outputs.redisName
    dbConnectionString: 'Server=tcp:${database.outputs.serverName},1433;Initial Catalog=${database.outputs.dbName};Persist Security Info=False;User ID=${dbAdminLogin};Password=${dbAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
    aiInstrumentationKey: ai.outputs.appInsightsInstrumentationKey
  }
}
