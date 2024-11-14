provider "google" {
  project = "teaching-record-system"
}

module "dfe_analytics" {
  count  = var.enable_dfe_analytics_federated_auth ? 1 : 0
  source = "./vendor/modules/aks//aks/dfe_analytics"

  azure_resource_prefix = var.azure_resource_prefix
  cluster               = var.cluster
  namespace             = var.namespace
  service_short         = var.service_short_name
  environment           = var.environment_name
  gcp_keyring           = "trs-key-ring"
  gcp_key               = "trs-key"
  gcp_taxonomy_id       = "4953620268226784157"
  gcp_policy_tag_id     = "2792096618541600484"
}
