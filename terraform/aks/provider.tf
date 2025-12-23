provider "azurerm" {
  skip_provider_registration = true

  features {}
}

provider "kubernetes" {
  host                   = module.cluster_data.kubernetes_host
  client_certificate     = module.cluster_data.kubernetes_client_certificate
  client_key             = module.cluster_data.kubernetes_client_key
  cluster_ca_certificate = module.cluster_data.kubernetes_cluster_ca_certificate

  dynamic "exec" {
    for_each = module.cluster_data.azure_RBAC_enabled ? [1] : []
    content {
      api_version = "client.authentication.k8s.io/v1beta1"
      command     = "kubelogin"
      args        = module.cluster_data.kubelogin_args
    }
  }
}

provider "statuscake" {
  api_token = module.infrastructure_secrets.map.STATUSCAKE-API-TOKEN
}

provider "airbyte" {
  # Configuration options
  server_url = var.airbyte_enabled ? "https://airbyte-${var.namespace}.${module.cluster_data.ingress_domain}/api/public/v1" : ""

  # client_id = var.airbyte_enabled ? data.azurerm_key_vault_secret.airbyte_client_id[0].value : ""
  # client_secret = var.airbyte_enabled ? data.azurerm_key_vault_secret.airbyte_client_secret[0].value: ""
  client_id     = var.airbyte_enabled ? module.infrastructure_secrets.map.AIRBYTE-CLIENT-ID : ""
  client_secret = var.airbyte_enabled ? module.infrastructure_secrets.map.AIRBYTE-CLIENT-SECRET: ""
}
