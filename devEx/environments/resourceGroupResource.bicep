param resourceGroupName string

targetScope = 'subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: 'eastus'
}

output resourceGroupName string = resourceGroup.name

