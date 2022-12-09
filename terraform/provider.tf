locals {
  azure_credentials  = try(jsondecode(var.azure_sp_credentials_json), null)
  monitoring_secrets = yamldecode(data.azurerm_key_vault_secret.secrets["MONITORING"].value)
}

provider "azurerm" {
  subscription_id            = try(local.azure_credentials.subscriptionId, null)
  client_id                  = try(local.azure_credentials.clientId, null)
  client_secret              = try(local.azure_credentials.clientSecret, null)
  tenant_id                  = try(local.azure_credentials.tenantId, null)
  skip_provider_registration = true

  features {}
}

provider "cloudfoundry" {
  api_url  = var.paas_api_url
  user     = data.azurerm_key_vault_secret.secrets["PAAS-USER"].value
  password = data.azurerm_key_vault_secret.secrets["PAAS-PASSWORD"].value
}

provider "statuscake" {
  api_token = local.monitoring_secrets.STATUSCAKE_PASSWORD
}
