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

var defaultTags = {
  'environment': environment
  'application': applicationName
}
var appWithEnv = '${applicationName}-${environment}'
var resourceGroupName = 'rg-${appWithEnv}'
var kvName = take('kv-${appWithEnv}', 24)
var apiName = 'as-api-${appWithEnv}'
var apiPlanName = 'asp-api-${appWithEnv}'
var guiName = 'as-gui-${appWithEnv}'
var guiPlanName = 'asp-gui-${appWithEnv}'
var aiName = 'ai-${appWithEnv}'
var redisName = 'redis-${appWithEnv}'
var serverName = 'sqls-${appWithEnv}'
var sqlDbName = 'sql-${appWithEnv}'
var storageName = take('stg${take(replace(applicationName, '-', ''),14)}${environment}', 24)

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
    name: storageName
    resourceTags: defaultTags
  }
}

module database 'modules/sql-server/sql-azure.bicep' = {
  name: 'sqlDb'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    tags: defaultTags
    administratorLogin: dbAdminLogin
    administratorPassword: dbAdminPassword
    sku: dbSku
    serverName: serverName
    sqlDBName: sqlDbName
  }
}

module redis 'modules/redis/redis.bicep' = {
  name: 'redis'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    name: redisName
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
  {
    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
    value: ai.outputs.appInsightsInstrumentationKey
  }
  {
    name: 'AzureAd:TenantId'
    value: tenant().tenantId
  }
]

module gui 'modules/app-service/app-service.bicep' = {
  name: 'gui'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    appName: guiName
    appServicePlanName: guiPlanName
    resourceTags: defaultTags
    environmentVariables: concat(applicationEnvironmentVariables, [
      {
        name: 'AzureAd:CallbackPath'
        value: callbackPath
      }
      {
        name: 'AzureAd:Domain'
        value: domain
      }
      {
        name: 'DownstreamApi:BaseUrl'
        value: 'https://${webApi.outputs.baseUrl}'
      }
      {
        name: 'DownstreamApi:Scopes'
        value: downstreamScopes
      }
    ])
    sku: appSku
  }
  dependsOn: [
    webApi
  ]
}

module webApi 'modules/app-service/app-service.bicep' = {
  name: 'webApi'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    appName: apiName
    appServicePlanName: apiPlanName
    resourceTags: defaultTags
    environmentVariables: concat(applicationEnvironmentVariables, [
      {
        name: 'AppOptions:FromEmail'
        value: fromEmail
      }
    ])
    sku: appSku
  }
}

module ai 'modules/application-insights/app-insights.bicep' = {
  name: 'instrumentation'
  scope: resourceGroup(rg.name)
  params: {
    location: location
    name: aiName
    resourceTags: defaultTags
  }
}

var secrets = [
  {
    name: 'AppOptions--GuiUrl'
    value: 'https://${gui.outputs.baseUrl}'
  }
  {
    name: 'AzureAd--ClientId'
    value: appClientId
  }
  {
    name: 'AzureAd--ClientSecret'
    value: appSecret
  }
  {
    name: 'AzureWebJobsSendGridApiKey'
    value: sendGridApiKey
  }
  {
    name: 'Connectionstrings--BookingDbConnectionString'
    value: 'Server=tcp:${database.outputs.serverName},1433;Initial Catalog=${sqlDbName};Persist Security Info=False;User ID=${dbAdminLogin};Password=${dbAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
]

module keyVault 'modules/key-vault/key-vault.bicep' = {
  name: 'keyVault'
  scope: resourceGroup(rg.name)
  dependsOn: [
    webApi
    database
    gui
    blobStorage
    redis
  ]
  params: {
    name: kvName
    resourceTags: defaultTags
    location: location
    storageName: storageName
    redisName: redisName
    secrets: secrets
    principalIds: [
      webApi.outputs.principalId
      gui.outputs.principalId
    ]
  }
}
