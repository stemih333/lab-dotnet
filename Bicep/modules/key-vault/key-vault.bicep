param location string
param resourceTags object
param sku object = {
  family: 'A'
  name: 'standard'
}
param principalIds array
param redisName string
param storageName string
param secrets array
param name string

var accessPolicies = [for id in principalIds: {
  tenantId: tenant().tenantId
  objectId: id
  permissions: {
    secrets: [
      'get'
      'list'
    ]
  }
}]

resource redis 'Microsoft.Cache/Redis@2019-07-01' existing = {
  name: redisName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' existing = {
  name: storageName
}

resource kv 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: name
  location: location
  tags: resourceTags
  properties: {
    sku: sku
    enableSoftDelete: false
    tenantId: tenant().tenantId
    accessPolicies: accessPolicies
  }
}

resource s 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = [for secret in secrets: {
  name: secret.name
  parent: kv
  properties: {
    value: secret.value
  }
}]

resource azureWebJobsStorageSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'AzureWebJobsStorage'
  parent: kv
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageName};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
  }
}

resource redisConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: 'Connectionstrings--Redis'
  parent: kv
  properties: {
    value: '${redisName}.redis.cache.windows.net:6380,password=${redis.listKeys().primaryKey},ssl=True,abortConnect=False'
  }
}
