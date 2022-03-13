param applicationName string
param environment string = 'dev'
param location string
param resourceTags object

var redisName = 'redis-${applicationName}-${environment}'

resource redis 'Microsoft.Cache/Redis@2019-07-01' = {
  name: redisName
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

output redisName string = redisName
