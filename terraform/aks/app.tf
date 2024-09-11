resource "azurerm_application_insights" "app" {
  name                = "${var.azure_resource_prefix}${var.service_short_name}${var.environment_short_name}${var.app_name != null && var.app_name != "" ? var.app_name : ""}ai"
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
    name      = "${var.service_name}-${local.app_name_suffix}-migrations"
    namespace = var.namespace
  }

  spec {
    template {
      metadata {}
      spec {
        container {
          name    = "cli"
          image   = var.docker_image
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

    backoff_limit = 0
  }

  wait_for_completion = true

  timeouts {
    create = "11m"
    update = "11m"
  }
}

module "api_application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "api"

  config_variables = {
    DataProtectionKeysContainerName = azurerm_storage_container.keys.name
    DistributedLockContainerName    = azurerm_storage_container.locks.name
    RecurringJobs__Enabled          = var.run_recurring_jobs
    SENTRY_ENVIRONMENT              = local.app_name_suffix
  }

  secret_variables = {
    ApplicationInsights__ConnectionString = azurerm_application_insights.app.connection_string
    ConnectionStrings__DefaultConnection  = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis              = module.redis.connection_string
    DATABASE_URL                          = module.postgres.url
    StorageConnectionString               = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
    Sentry__Dsn                           = module.infrastructure_secrets.map.SENTRY-DSN
    SharedConfig                          = module.infrastructure_secrets.map.SharedConfig
  }
}

module "api_application" {
  source     = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"
  depends_on = [kubernetes_job.migrations]

  name   = "api"
  is_web = true

  namespace    = var.namespace
  environment  = local.app_name_suffix
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.api_application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.api_application_configuration.kubernetes_secret_name

  docker_image   = var.docker_image
  command        = ["/bin/ash", "-c", "cd /Apps/Api/; dotnet TeachingRecordSystem.Api.dll;"]
  web_port       = 3000
  probe_path     = "/health"
  replicas       = var.api_replicas
  max_memory     = var.api_max_memory
  enable_logit   = var.enable_logit
  enable_gcp_wif = true
}

module "authz_application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "authz"

  config_variables = {
    DataProtectionKeysContainerName = azurerm_storage_container.keys.name
    SENTRY_ENVIRONMENT              = local.app_name_suffix
    DUMMY                           = "Dummy variable to force new Kubernetes config map to be created"
  }

  secret_variables = {
    ApplicationInsights__ConnectionString = azurerm_application_insights.app.connection_string
    ConnectionStrings__DefaultConnection  = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis              = "${module.redis.connection_string},defaultDatabase=1"
    DATABASE_URL                          = module.postgres.url
    Sentry__Dsn                           = module.infrastructure_secrets.map.SENTRY-DSN
    SharedConfig                          = module.infrastructure_secrets.map.SharedConfig
    StorageConnectionString               = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
  }
}

module "authz_application" {
  source     = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"
  depends_on = [kubernetes_job.migrations]

  name   = "authz"
  is_web = true

  namespace    = var.namespace
  environment  = local.app_name_suffix
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.authz_application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.authz_application_configuration.kubernetes_secret_name

  docker_image   = var.docker_image
  command        = ["/bin/ash", "-c", "cd /Apps/AuthorizeAccess/; dotnet TeachingRecordSystem.AuthorizeAccess.dll;"]
  web_port       = 3000
  probe_path     = "/health"
  replicas       = var.authz_replicas
  max_memory     = var.authz_max_memory
  enable_logit   = var.enable_logit
  enable_gcp_wif = true
}

module "ui_application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "ui"

  config_variables = {
    DataProtectionKeysContainerName = azurerm_storage_container.keys.name
    SENTRY_ENVIRONMENT              = local.app_name_suffix
  }

  secret_variables = {
    ApplicationInsights__ConnectionString = azurerm_application_insights.app.connection_string
    ConnectionStrings__DefaultConnection  = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis              = "${module.redis.connection_string},defaultDatabase=1"
    DATABASE_URL                          = module.postgres.url
    Sentry__Dsn                           = module.infrastructure_secrets.map.SENTRY-DSN
    SharedConfig                          = module.infrastructure_secrets.map.SharedConfig
    StorageConnectionString               = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
  }
}

module "ui_application" {
  source     = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"
  depends_on = [kubernetes_job.migrations]

  name   = "ui"
  is_web = true

  namespace    = var.namespace
  environment  = local.app_name_suffix
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.ui_application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.ui_application_configuration.kubernetes_secret_name

  docker_image                 = var.docker_image
  command                      = ["/bin/ash", "-c", "cd /Apps/SupportUi/; dotnet TeachingRecordSystem.SupportUi.dll;"]
  web_port                     = 3000
  probe_path                   = "/health"
  replicas                     = var.ui_replicas
  enable_logit                 = var.enable_logit
  enable_prometheus_monitoring = var.enable_prometheus_monitoring
  enable_gcp_wif               = true
}

module "worker_application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "worker"

  config_variables = {
    DistributedLockContainerName = azurerm_storage_container.locks.name
    DqtReporting__RunService     = var.run_dqt_reporting_service
    SENTRY_ENVIRONMENT           = local.app_name_suffix
  }

  secret_variables = {
    ApplicationInsights__ConnectionString     = azurerm_application_insights.app.connection_string
    ConnectionStrings__DefaultConnection      = module.postgres.dotnet_connection_string
    DqtReporting__ReportingDbConnectionString = local.reporting_db_connection_string
    DATABASE_URL                              = module.postgres.url
    Sentry__Dsn                               = module.infrastructure_secrets.map.SENTRY-DSN
    StorageConnectionString                   = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
    SharedConfig                              = module.infrastructure_secrets.map.SharedConfig
  }
}

module "worker_application" {
  source     = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"
  depends_on = [kubernetes_job.migrations]

  name   = "worker"
  is_web = false

  namespace    = var.namespace
  environment  = local.app_name_suffix
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.worker_application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.worker_application_configuration.kubernetes_secret_name

  docker_image                 = var.docker_image
  command                      = ["/bin/ash", "-c", "cd /Apps/Worker/; dotnet TeachingRecordSystem.Worker.dll;"]
  replicas                     = var.worker_replicas
  max_memory                   = var.worker_max_memory
  enable_logit                 = var.enable_logit
  enable_prometheus_monitoring = var.enable_prometheus_monitoring
  enable_gcp_wif               = true
}
