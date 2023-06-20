data "azurerm_key_vault" "vault" {
  name                = var.key_vault_name
  resource_group_name = var.resource_group_name
}

data "azurerm_key_vault_secrets" "secrets" {
  key_vault_id = data.azurerm_key_vault.vault.id
}

data "azurerm_key_vault_secret" "secrets" {
  key_vault_id = data.azurerm_key_vault.vault.id
  for_each     = toset(data.azurerm_key_vault_secrets.secrets.names)
  name         = each.key
}

data "cloudfoundry_space" "space" {
  name     = var.paas_space
  org_name = var.paas_org_name
}

data "cloudfoundry_domain" "cloudapps" {
  name = "london.cloudapps.digital"
}

data "cloudfoundry_domain" "internal" {
  name = "apps.internal"
}

data "cloudfoundry_domain" "education_gov_uk" {
  name = "education.gov.uk"
}

data "cloudfoundry_service" "postgres" {
  name = "postgres"
}

data "cloudfoundry_service" "redis" {
  name = "redis"
}

locals {
  paas_egress_ips = [
    "35.178.62.180",
    "18.130.41.69",
    "35.177.73.214"
  ]
}
