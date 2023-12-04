resource "azurerm_application_insights" "app" {
  name                = "${var.azure_resource_prefix}${var.service_short_name}${var.environment_short_name}ai"
  resource_group_name = var.resource_group_name
  location            = var.region
  application_type    = "web"

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "kubernetes_job" "migrations" {
  metadata {
    name      = "${var.service_name}-${var.environment_name}-migrations"
    namespace = var.namespace
  }

  spec {
    template {
      metadata {}
      spec {
        container {
          name    = "cli"
          image   = var.cli_docker_image
          command = ["trscli"]
          args    = ["migrate-db", "--connection-string", "$(CONNECTION_STRING)"]

          env {
            name  = "CONNECTION_STRING"
            value = module.postgres.dotnet_connection_string
          }
        }

        restart_policy = "Never"
      }
    }

    backoff_limit = 1
  }

  wait_for_completion = true
}

module "api_application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = var.environment_name
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "api"

  config_variables = {
    PlatformEnvironment             = var.environment_name
    DistributedLockContainerName    = azurerm_storage_container.locks.name
    DqtReporting__RunService        = var.run_dqt_reporting_service
    RecurringJobs__Enabled          = var.run_recurring_jobs
    DataProtectionKeysContainerName = azurerm_storage_container.keys.name
  }

  secret_variables = {
    ApplicationInsights__ConnectionString     = azurerm_application_insights.app.connection_string
    ConnectionStrings__DefaultConnection      = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis                  = module.redis.connection_string
    StorageConnectionString                   = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
    DqtReporting__ReportingDbConnectionString = local.reporting_db_connection_string
  }
}

module "api_application" {
  source     = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"
  depends_on = [kubernetes_job.migrations, kubernetes_job.reporting_migrations]

  name   = "api"
  is_web = true

  namespace    = var.namespace
  environment  = var.environment_name
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.api_application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.api_application_configuration.kubernetes_secret_name

  docker_image = var.api_docker_image
  web_port     = 80
  probe_path   = "/health"
  replicas     = var.api_replicas
  max_memory   = var.api_max_memory
}

module "ui_application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = var.environment_name
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "ui"

  config_variables = {
    PlatformEnvironment             = var.environment_name
    DataProtectionKeysContainerName = azurerm_storage_container.keys.name
  }

  secret_variables = {
    ConnectionStrings__DefaultConnection = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis             = "${module.redis.connection_string},defaultDatabase=1"
    StorageConnectionString              = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
  }
}

module "ui_application" {
  source     = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"
  depends_on = [kubernetes_job.migrations]

  name   = "ui"
  is_web = true

  namespace    = var.namespace
  environment  = var.environment_name
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.ui_application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.ui_application_configuration.kubernetes_secret_name

  docker_image = var.ui_docker_image
  web_port     = 80
  probe_path   = "/health"
  replicas     = var.ui_replicas
}

module "worker_application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = var.environment_name
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "worker"

  config_variables = {
    PlatformEnvironment = var.environment_name
  }

  secret_variables = {
    ConnectionStrings__DefaultConnection = module.postgres.dotnet_connection_string
    StorageConnectionString              = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
  }
}

module "worker_application" {
  source     = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"
  depends_on = [kubernetes_job.migrations, kubernetes_job.reporting_migrations]

  name   = "worker"
  is_web = false

  namespace    = var.namespace
  environment  = var.environment_name
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.worker_application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.worker_application_configuration.kubernetes_secret_name

  docker_image = var.worker_docker_image
  replicas     = var.worker_replicas
  max_memory   = var.worker_max_memory
}
