

var saNames = [
  'myStorageAccount1'
  'myStorageAccount2'
  'myStorageAccount3'
  'myStorageAccount4'
  'myStorageAccount5'
  'myStorageAccount6'
  'myStorageAccount7'
  'myStorageAccount8'
  'myStorageAccount9'
  'myStorageAccount10'
  'myStorageAccount11'
  'myStorageAccount12'
  'myStorageAccount13'
]


module sa 'storageAccountResource.bicep' = [for saName in saNames:  {
  name: 'StorageAccountResource-${saName}'
  scope: resourceGroup()
  params: {
    name: saName
  }
}
]
