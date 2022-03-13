param applicationName string
param environment string = 'dev'
param location string
param resourceTags object

var appInsightsResourceName = 'ai-${applicationName}-${environment}'

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: appInsightsResourceName
  location: location
  tags: resourceTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
