param name string
param location string
param resourceTags object

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: name
  location: location
  tags: resourceTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
