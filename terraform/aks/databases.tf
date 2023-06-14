locals {
  postgres_connection_string = "Server=${module.postgres.host};Database=${module.postgres.name};Port=${module.postgres.port};User Id=${module.postgres.username};Password='${module.postgres.password}';Ssl Mode=Require;Trust Server Certificate=true"
}

module "redis" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/redis?ref=main"

  namespace             = var.namespace
  environment           = var.environment_name
  azure_resource_prefix = var.azure_resource_prefix
  service_name          = var.service_name
  service_short         = var.service_short_name
  config_short          = var.environment_short_name

  cluster_configuration_map = module.cluster_data.configuration_map

  use_azure               = var.deploy_azure_backing_services
  azure_enable_monitoring = var.enable_monitoring
}

module "postgres" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/postgres?ref=testing"

  namespace             = var.namespace
  environment           = var.environment_name
  azure_resource_prefix = var.azure_resource_prefix
  service_name          = var.service_name
  service_short         = var.service_short_name
  config_short          = var.environment_short_name

  cluster_configuration_map = module.cluster_data.configuration_map

  use_azure               = var.deploy_azure_backing_services
  azure_enable_monitoring = var.enable_monitoring
  azure_extensions        = ["pg_stat_statements"]
}
