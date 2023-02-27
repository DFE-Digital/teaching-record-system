resource "azurerm_storage_account" "app-storage" {
  name                              = var.qualified_teachers_api_storage_account_name
  location                          = "West Europe"
  resource_group_name               = var.resource_group_name
  account_replication_type          = var.environment_name != "production" ? "LRS" : "GRS"
  account_tier                      = "Standard"
  account_kind                      = "BlobStorage"
  min_tls_version                   = "TLS1_2"
  infrastructure_encryption_enabled = true

  blob_properties {
    last_access_time_enabled = true
  }
}

resource "azurerm_storage_container" "certificates" {
  name                  = "certificates"
  storage_account_name  = azurerm_storage_account.app-storage.name
  container_access_type = "private"
}

locals {
  StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app-storage.name};AccountKey=${azurerm_storage_account.app-storage.primary_access_key}"
}
