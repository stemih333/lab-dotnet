param name string
param location string
param resourceTags object

resource redis 'Microsoft.Cache/Redis@2019-07-01' = {
  name: name
  location: location
  tags: resourceTags
  properties: {
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    sku: {
      capacity: 0
      family: 'C'
      name: 'Standard'
    }
  }
}
