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

module "application_configuration" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application_configuration?ref=testing"

  namespace              = var.namespace
  environment            = var.environment_name
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short_name
  config_short           = var.environment_short_name
  secret_key_vault_short = "app"

  config_variables = {
    Platform                                       = "AKS"
    PlatformEnvironment                            = var.environment_name
    DistributedLockContainerName                   = azurerm_storage_container.locks.name
    DqtReporting__RunService                       = "false"
    GetAnIdentity__RunLinkTrnToIdentityUserService = "false"
    RecurringJobs__Enabled                         = "false"
  }

  secret_variables = {
    ApplicationInsights__ConnectionString = azurerm_application_insights.app.connection_string
    ConnectionStrings__DefaultConnection  = module.postgres.dotnet_connection_string
    ConnectionStrings__Redis              = module.redis.connection_string
    StorageConnectionString               = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app_storage.name};AccountKey=${azurerm_storage_account.app_storage.primary_access_key}"
  }
}

module "web_application" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/application?ref=testing"

  name   = "web"
  is_web = true

  namespace    = var.namespace
  environment  = var.environment_name
  service_name = var.service_name

  cluster_configuration_map = module.cluster_data.configuration_map

  kubernetes_config_map_name = module.application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.application_configuration.kubernetes_secret_name

  docker_image = var.app_docker_image
  web_port     = 80
  probe_path   = "/health"
}
