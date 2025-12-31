module "migrations_job_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "inf"

  config_variables = merge(local.federated_auth_configmap, {
    ENVIRONMENT_NAME         = var.environment_name,
    AIRBYTE_CONNECTION_ID    = var.airbyte_enabled ? module.airbyte[0].airbyte_connection_id : "",
    AIRBYTE_API_BASE_ADDRESS = var.airbyte_enabled ? local.airbyte_server_url : ""
  })

  secret_variables = merge(local.federated_auth_secrets, {
    CONNECTION_STRING = module.postgres.dotnet_connection_string
  })
}

module "migrations" {
  source = "./vendor/modules/aks//aks/job_configuration"

  namespace    = var.namespace
  environment  = local.app_name_suffix
  service_name = var.service_name
  docker_image = var.docker_image
  commands     = ["trscli"]
  arguments = [
    "deploy-analytics",
    "--connection-string",
    "$(CONNECTION_STRING)",
    "--airbyte-connection-id",
    "$(AIRBYTE_CONNECTION_ID)",
    "--airbyte-client-id",
    "$(AIRBYTE-CLIENT-ID)",
    "--airbyte-client-secret",
    "$(AIRBYTE-CLIENT-SECRET)",
    "--airbyte-api-base-address",
    "$(AIRBYTE_API_BASE_ADDRESS)",
    "--hidden-policy-tag-name",
    module.airbyte[0].hidden_policy_tag_name,
    "--project-id",
    "$(DfeAnalytics__ProjectId)",
    "--dataset-id",
    local.gcp_dataset_name,
    "--google-credentials",
    module.airbyte[0].google_cloud_credentials
  ]
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

  config_variables = local.shared_config

  secret_variables = local.shared_secrets
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

  docker_image    = var.docker_image
  command         = ["/bin/ash", "-c", "cd /Apps/Api/; dotnet TeachingRecordSystem.Api.dll;"]
  web_port        = 3000
  probe_path      = "/health"
  replicas        = var.api_replicas
  max_memory      = var.api_max_memory
  enable_logit    = var.enable_logit
  enable_gcp_wif  = true
  run_as_non_root = var.run_as_non_root
}

module "authz_application_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "authz"

  config_variables = merge(local.shared_config, local.federated_auth_configmap, { App = "AuthorizeAccess" })

  secret_variables = merge(local.shared_secrets, local.federated_auth_secrets)
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

  docker_image    = var.docker_image
  command         = ["/bin/ash", "-c", "cd /Apps/AuthorizeAccess/; dotnet TeachingRecordSystem.AuthorizeAccess.dll;"]
  web_port        = 3000
  probe_path      = "/health"
  replicas        = var.authz_replicas
  max_memory      = var.authz_max_memory
  enable_logit    = var.enable_logit
  enable_gcp_wif  = true
  run_as_non_root = var.run_as_non_root
}

module "ui_application_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "ui"

  config_variables = merge(local.shared_config, { app = "SupportUi" })

  secret_variables = local.shared_secrets
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
  run_as_non_root              = var.run_as_non_root
}

module "worker_application_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = local.app_name_suffix
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "worker"

  config_variables = merge(local.shared_config, { app = "Worker" })

  secret_variables = merge(local.shared_secrets, {
    DqtReporting__ReportingDbConnectionString = local.reporting_db_connection_string
  })
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
  max_memory                   = "4Gi"
  enable_logit                 = var.enable_logit
  enable_prometheus_monitoring = var.enable_prometheus_monitoring
  enable_gcp_wif               = true
  run_as_non_root              = var.run_as_non_root
}

locals {
  shared_config = {
    DataProtectionKeysContainerName = azurerm_storage_container.keys.name
    DistributedLockContainerName    = azurerm_storage_container.locks.name
    ENVIRONMENT_NAME                = var.environment_name
    SENTRY_ENVIRONMENT              = local.app_name_suffix
  }

  shared_secrets = merge({
    ConnectionStrings__DefaultConnection = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis             = "${module.redis.connection_string},defaultDatabase=1"
    DATABASE_URL                         = module.postgres.url
    Sentry__Dsn                          = module.infrastructure_secrets.map.SENTRY-DSN
    StorageConnectionString              = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
    SftpStorageName                      = "${azurerm_storage_account.sftp_storage.name}"
    SftpStorageAccessKey                 = "${azurerm_storage_account.sftp_storage.primary_access_key}"
  }, { for k, v in module.infrastructure_secrets.map : replace(k, "--", "__") => v })
}
