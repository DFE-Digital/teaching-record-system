locals {
  azure_credentials  = jsondecode(var.azure_sp_credentials_json)
  monitoring_secrets = yamldecode(data.azurerm_key_vault_secret.secrets["MONITORING"].value)
}

provider "azurerm" {
  subscription_id            = local.azure_credentials.subscriptionId
  client_id                  = local.azure_credentials.clientId
  client_secret              = local.azure_credentials.clientSecret
  tenant_id                  = local.azure_credentials.tenantId
  skip_provider_registration = true

  features {}
}

provider "cloudfoundry" {
  api_url  = var.paas_api_url
  user     = data.azurerm_key_vault_secret.secrets["PAAS-USER"].value
  password = data.azurerm_key_vault_secret.secrets["PAAS-PASSWORD"].value
}

provider "statuscake" {
  username = local.monitoring_secrets.STATUSCAKE_USERNAME
  apikey   = local.monitoring_secrets.STATUSCAKE_PASSWORD
}
