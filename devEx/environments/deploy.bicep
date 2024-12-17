param resourceGroupName string

module resource 'resourceGroupResource.bicep' = {
  name: resourceGroupName
  scope:subscription()
  params: {
    resourceGroupName: resourceGroupName
  }
}



module sa 'storageAccountResource.bicep' = {
  name: 'myStorageAccount'
  scope: resourceGroup(resourceGroupName)
  params: {
    
  }
}
