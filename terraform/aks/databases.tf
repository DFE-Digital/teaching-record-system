module "redis" {
  source = "./vendor/modules/aks//aks/redis"

  namespace             = var.namespace
  environment           = local.app_name_suffix
  azure_resource_prefix = var.azure_resource_prefix
  service_name          = var.service_name
  service_short         = var.service_short_name
  config_short          = var.environment_short_name

  cluster_configuration_map = module.cluster_data.configuration_map

  use_azure               = var.deploy_azure_backing_services
  azure_enable_monitoring = var.enable_monitoring
  server_version          = 6
  azure_capacity          = var.redis_capacity
  azure_family            = var.redis_family
  azure_sku_name          = var.redis_sku_name
}

module "postgres" {
  source = "./vendor/modules/aks//aks/postgres"

  namespace             = var.namespace
  environment           = local.app_name_suffix
  azure_resource_prefix = var.azure_resource_prefix
  service_name          = var.service_name
  service_short         = var.service_short_name
  config_short          = var.environment_short_name

  cluster_configuration_map = module.cluster_data.configuration_map

  use_azure                      = var.deploy_azure_backing_services
  azure_enable_monitoring        = true
  azure_extensions               = ["pg_stat_statements", "pg_trgm", "btree_gin", "btree_gist"]
  server_version                 = var.postgres_server_version
  azure_sku_name                 = var.postgres_flexible_server_sku
  azure_enable_high_availability = var.postgres_enable_high_availability
  azure_storage_mb               = var.postgres_azure_storage_mb

  use_airbyte = var.pg_airbyte_enabled
}

resource "azurerm_postgresql_flexible_server_configuration" "wal_level" {
  name      = "wal_level"
  server_id = module.postgres.azure_server_id
  value     = "logical"
}

resource "azurerm_postgresql_flexible_server_configuration" "shared_preload_libraries" {
  name      = "shared_preload_libraries"
  server_id = module.postgres.azure_server_id
  value     = "pg_cron,pg_stat_statements"
}

resource "azurerm_postgresql_flexible_server_configuration" "hot_standby_feedback" {
  name      = "hot_standby_feedback"
  server_id = module.postgres.azure_server_id
  value     = "on"
}
