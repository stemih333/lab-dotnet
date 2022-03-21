param location string
param resourceTags object
param name string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: name
  location: location
  tags: resourceTags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-08-01' = {
  name: 'default'
  parent: storageAccount
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2021-08-01' = {
  name: 'default'
  parent: storageAccount
}

resource attachments 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  name: 'attachments'
  parent: blobService
}

resource internalFiles 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-04-01' = {
  name: 'internal-files'
  parent: blobService
}

resource toBeBooked 'Microsoft.Storage/storageAccounts/queueServices/queues@2021-08-01' = {
  name: 'tobebooked'
  parent: queueService
}

output storageAccountName string = storageAccount.name
output id string = storageAccount.id
