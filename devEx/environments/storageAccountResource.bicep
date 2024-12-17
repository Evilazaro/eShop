
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: uniqueString('myStorageAccount',subscription().id)
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}
