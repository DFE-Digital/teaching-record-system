resource "azurerm_storage_account" "safe_storage" {
  name                              = "${var.azure_resource_prefix}${var.service_short_name}${var.environment_short_name}${var.app_name != null && var.app_name != "" ? var.app_name : ""}safesa"
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

resource "azurerm_security_center_storage_defender" "safe_storage" {
  storage_account_id                          = azurerm_storage_account.safe_storage.id
  malware_scanning_on_upload_enabled          = true
  malware_scanning_on_upload_cap_gb_per_month = 100
  override_subscription_settings_enabled      = true
}

resource "azurerm_storage_container" "safe_uploads" {
  name                  = "uploads"
  storage_account_name  = azurerm_storage_account.safe_storage.name
  container_access_type = "private"
}
