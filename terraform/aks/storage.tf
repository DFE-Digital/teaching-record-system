resource "azurerm_storage_account" "app_storage" {
  name                              = "${var.azure_resource_prefix}${var.service_short_name}${var.environment_short_name}sa"
  location                          = var.region
  resource_group_name               = var.resource_group_name
  account_replication_type          = var.environment_name != "production" ? "LRS" : "GRS"
  account_tier                      = "Standard"
  account_kind                      = "StorageV2"
  min_tls_version                   = "TLS1_2"
  infrastructure_encryption_enabled = true

  blob_properties {
    last_access_time_enabled = true
  }

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_storage_container" "certificates" {
  name                  = "certificates"
  storage_account_name  = azurerm_storage_account.app_storage.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "locks" {
  name                  = "locks"
  storage_account_name  = azurerm_storage_account.app_storage.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "keys" {
  name                  = "keys"
  storage_account_name  = azurerm_storage_account.app_storage.name
  container_access_type = "private"
}
