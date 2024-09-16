module "infrastructure_secrets" {
  source = "./vendor/modules/aks//aks/secrets"

  azure_resource_prefix = var.azure_resource_prefix
  service_short         = var.service_short_name
  config_short          = var.environment_short_name
  key_vault_short       = "inf"
}
