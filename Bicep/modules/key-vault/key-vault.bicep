param apiPrincipalId string
param guiPrincipalId string
param location string
param resourceTags object
param sku object = {
  family: 'A'
  name: 'standard'
}

param fromEmail string
param domain string
param callbackPath string
param dbConnectionString string
param downstreamBaseUrl string
param downstreamScopes string
param redisConnectionString string
param azureWebJobsStorage string
param guiUrl string
param clientId string
@secure()
param secret string
@secure()
param sendGridApiKey string
param aiInstrumentationKey string
param kvName string

resource kv 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: kvName
  location: location
  tags: resourceTags
  properties: {
    sku: sku
    tenantId: tenant().tenantId
    accessPolicies: [
      {
        tenantId: tenant().tenantId
        objectId: apiPrincipalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: tenant().tenantId
        objectId: guiPrincipalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}


resource fromEmailSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AppOptions--FromEmail'
  parent: kv
  properties: {
    value: fromEmail
  }
}

resource fromGuiUrlSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AppOptions--GuiUrl'
  parent: kv
  properties: {
    value: 'https://${guiUrl}'
  }
}

resource clientIdSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureAd--ClientId'
  parent: kv
  properties: {
    value: clientId
  }
}

resource clientSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureAd--ClientSecret'
  parent: kv
  properties: {
    value: secret
  }
}

resource tenantIdSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureAd--TenantId'
  parent: kv
  properties: {
    value: tenant().tenantId
  }
}

resource callbackPathSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureAd--CallbackPath'
  parent: kv
  properties: {
    value: callbackPath
  }
}

resource domainSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureAd--Domain'
  parent: kv
  properties: {
    value: domain
  }
}

resource sendGridApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureWebJobsSendGridApiKey'
  parent: kv
  properties: {
    value: sendGridApiKey
  }
}

resource azureWebJobsStorageSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureWebJobsStorage'
  parent: kv
  properties: {
    value: azureWebJobsStorage
  }
}

resource dbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'Connectionstrings--BookingDbConnectionString'
  parent: kv
  properties: {
    value: dbConnectionString
  }
}

resource redisConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'Connectionstrings--Redis'
  parent: kv
  properties: {
    value: redisConnectionString
  }
}

resource downstreamBaseUrlSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'DownstreamApi--BaseUrl'
  parent: kv
  properties: {
    value: 'https://${downstreamBaseUrl}'
  }
}

resource downstreamScopesSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'DownstreamApi--Scopes'
  parent: kv
  properties: {
    value: downstreamScopes
  }
}

resource aiInstrumentationKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'ApplicationInsights--InstrumentationKey'
  parent: kv
  properties: {
    value: aiInstrumentationKey
  }
}

output vaultUri string = kv.properties.vaultUri
output vaultName string = kvName
