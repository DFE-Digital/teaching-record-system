module "airbyte" {
  source = "./vendor/modules/aks//aks/airbyte"

  count = var.airbyte_enabled ? 1 : 0

  environment           = local.app_name_suffix
  azure_resource_prefix = var.azure_resource_prefix
  service_short         = var.service_short_name
  service_name          = var.service_name
  docker_image          = var.docker_image
  postgres_version      = var.postgres_server_version
  postgres_url          = module.postgres.url

  host_name     = module.postgres.host
  database_name = module.postgres.name
  workspace_id  = module.infrastructure_secrets.map.AIRBYTE-WORKSPACE-ID
  client_id     = module.infrastructure_secrets.map.AIRBYTE-CLIENT-ID
  client_secret = module.infrastructure_secrets.map.AIRBYTE-CLIENT-SECRET
  repl_password = module.infrastructure_secrets.map.AIRBYTE-REPLICATION-PASSWORD

  server_url        = "https://airbyte-${var.namespace}.${module.cluster_data.ingress_domain}"
  connection_status = var.airbyte_connection_status

  cluster           = var.cluster
  namespace         = var.namespace
  gcp_taxonomy_id   = "4953620268226784157"
  gcp_policy_tag_id = "2818002780538442239"
  gcp_keyring       = "trs-key-ring"
  gcp_key           = "trs-key"
  gcp_bq_sa         = module.infrastructure_secrets.map.AIRBYTE-BQ-SA

  gcp_dataset_internal = "airbyte_internal"

  config_map_ref = module.migrations_job_configuration.kubernetes_config_map_name
  secret_ref     = module.migrations_job_configuration.kubernetes_secret_name
  cpu            = module.cluster_data.configuration_map.cpu_min

  use_azure = var.deploy_azure_backing_services

  is_rails_application         = false
  is_dotnet_application        = true
  dotnet_application_directory = "/Apps/TrsCli"
}

locals {
  gcp_dataset_name = replace("${var.service_short_name}_airbyte_${local.app_name_suffix}", "-", "_")
}
