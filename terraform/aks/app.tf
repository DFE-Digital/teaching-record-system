module "migrations_job_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "inf"

  config_variables = {
    DUMMY = ""
  }
  secret_variables = {
    CONNECTION_STRING = module.postgres.dotnet_connection_string
  }
}

module "migrations" {
  source = "./vendor/modules/aks//aks/job_configuration"

  namespace    = var.namespace
  environment  = local.app_name_suffix
  service_name = var.service_name
  docker_image = var.docker_image
  commands     = ["trscli"]
  arguments    = ["migrate-db", "--connection-string", "$(CONNECTION_STRING)"]
  job_name     = "migrations"
  enable_logit = var.enable_logit

  config_map_ref = module.migrations_job_configuration.kubernetes_config_map_name
  secret_ref     = module.migrations_job_configuration.kubernetes_secret_name
  cpu            = module.cluster_data.configuration_map.cpu_min
}

module "api_application_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

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
    ConnectionStrings__DefaultConnection = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis             = module.redis.connection_string
    DATABASE_URL                         = module.postgres.url
    StorageConnectionString              = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
    Sentry__Dsn                          = module.infrastructure_secrets.map.SENTRY-DSN
    SharedConfig                         = module.infrastructure_secrets.map.SharedConfig
  }
}

module "api_application" {
  source = "./vendor/modules/aks//aks/application"

  depends_on = [module.migrations]

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
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "authz"

  config_variables = merge({
    DataProtectionKeysContainerName = azurerm_storage_container.keys.name
    SENTRY_ENVIRONMENT              = local.app_name_suffix
    DUMMY                           = "Dummy variable to force new Kubernetes config map to be created"
  }, local.federated_auth_configmap)

  secret_variables = merge({
    ConnectionStrings__DefaultConnection = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis             = "${module.redis.connection_string},defaultDatabase=1"
    DATABASE_URL                         = module.postgres.url
    Sentry__Dsn                          = module.infrastructure_secrets.map.SENTRY-DSN
    SharedConfig                         = module.infrastructure_secrets.map.SharedConfig
    StorageConnectionString              = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
  }, local.federated_auth_secrets)
}

module "authz_application" {
  source = "./vendor/modules/aks//aks/application"

  depends_on = [module.migrations]

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
  source = "./vendor/modules/aks//aks/application_configuration"

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
    ConnectionStrings__DefaultConnection = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis             = "${module.redis.connection_string},defaultDatabase=1"
    DATABASE_URL                         = module.postgres.url
    Sentry__Dsn                          = module.infrastructure_secrets.map.SENTRY-DSN
    SharedConfig                         = module.infrastructure_secrets.map.SharedConfig
    StorageConnectionString              = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
  }
}

module "ui_application" {
  source = "./vendor/modules/aks//aks/application"

  depends_on = [module.migrations]

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
  source = "./vendor/modules/aks//aks/application_configuration"

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
    ConnectionStrings__DefaultConnection      = module.postgres.dotnet_connection_string
    DqtReporting__ReportingDbConnectionString = local.reporting_db_connection_string
    DATABASE_URL                              = module.postgres.url
    Sentry__Dsn                               = module.infrastructure_secrets.map.SENTRY-DSN
    StorageConnectionString                   = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
    SharedConfig                              = module.infrastructure_secrets.map.SharedConfig
  }
}

module "worker_application" {
  source = "./vendor/modules/aks//aks/application"

  depends_on = [module.migrations]

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
