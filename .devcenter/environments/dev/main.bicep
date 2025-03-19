var workloadName = 'identityProvider'

@allowed([
  'dev'
  'prod'
])
param environment string = 'dev'

resource rg 'Microsoft.Resources/resourceGroups@2024-11-01' existing = {
  name: resourceGroup().name
  scope: subscription()
}

module monitoring 'logAnalyticsResource.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    name: '${workloadName}-monitoring'
    tags: {
      environment: 'dev'
      name: workloadName
    }
  }
}

module webapp 'appServiceResource.bicep' = {
  name: 'webapp'
  scope: rg
  params: {
    name: workloadName
    environment: environment
    ConnectionString: monitoring.outputs.ConnectionString
    InstrumentationKey: monitoring.outputs.InstrumentationKey
  }
}

output RESOURCE_NAME string = webapp.outputs.webappName
output WEB_APP_URL string = webapp.outputs.webappUrl
output identityProvider string = webapp.outputs.webappName
