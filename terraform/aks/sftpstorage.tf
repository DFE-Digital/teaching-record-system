resource "azurerm_storage_account" "sftp_storage" {
  name                              = "${var.azure_resource_prefix}${var.service_short_name}${var.environment_short_name}${var.app_name != null && var.app_name != "" ? var.app_name : ""}sftpsa"
  location                          = var.region
  resource_group_name               = var.resource_group_name
  account_replication_type          = var.environment_name != "production" ? "LRS" : "GRS"
  account_tier                      = "Standard"
  account_kind                      = "StorageV2"
  min_tls_version                   = "TLS1_2"
  infrastructure_encryption_enabled = true
  # Enable hierarchical namespace & SFTP
  is_hns_enabled                  = true
  sftp_enabled                    = true
  allow_nested_items_to_be_public = false

  # Restrict access to allowed IPs
  network_rules {
    default_action = "Deny"
    bypass         = ["AzureServices"]
    ip_rules = [
    ]
  }
  blob_properties {
    last_access_time_enabled = true
  }
  lifecycle {
    ignore_changes = [
      tags,
      network_rules
    ]
  }
}

# Enabling sftp on the sa prevents being able to create a storage container 
# because enabling hns/sftp switches to datalake api rather than blob storage api
# 
# This container is created manually for now.
#resource "azurerm_storage_container" "sftp_ewc" {
#  name                  = "ewc-integrations"
#  storage_account_name  = azurerm_storage_account.sftp_storage.name
#  container_access_type = "private"
#}
