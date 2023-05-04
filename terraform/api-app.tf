locals {
  api_app_config = {
    AppConfig = jsonencode(merge(
      jsondecode(data.azurerm_key_vault_secret.secrets["APP-CONFIG"].value),
      {
        "ApplicationInsights" : {
          "ConnectionString" : azurerm_application_insights.api_app_insights.connection_string
        }
      }
    )),
    AppVersion                                = var.api_app_version,
    PaasEnvironment                           = var.environment_name,
    StorageConnectionString                   = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app-storage.name};AccountKey=${azurerm_storage_account.app-storage.primary_access_key}",
    DqtReporting__ReportingDbConnectionString = length(azurerm_mssql_server.reporting_server) == 1 ? "Data Source=tcp:${azurerm_mssql_server.reporting_server[0].fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.reporting_db[0].name};Persist Security Info=False;User ID=${azurerm_mssql_server.reporting_server[0].administrator_login};Password=${yamldecode(data.azurerm_key_vault_secret.secrets["REPORTING-DB"].value)["PASSWORD"]};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" : "",
    DistributedLockContainerName              = local.distributed_lock_container_name
  }

  logstash_endpoint               = data.azurerm_key_vault_secret.secrets["LOGSTASH-ENDPOINT"].value
  distributed_lock_container_name = "locks"
}

resource "cloudfoundry_route" "api_public" {
  domain   = data.cloudfoundry_domain.cloudapps.id
  hostname = var.api_app_name
  space    = data.cloudfoundry_space.space.id
}

resource "cloudfoundry_route" "api_internal" {
  count    = local.configure_prometheus_network_policy
  domain   = data.cloudfoundry_domain.internal.id
  hostname = var.api_app_name
  space    = data.cloudfoundry_space.space.id
}

resource "cloudfoundry_route" "api_education" {
  for_each = toset(var.hostnames)
  domain   = data.cloudfoundry_domain.education_gov_uk.id
  space    = data.cloudfoundry_space.space.id
  hostname = each.value
}

resource "cloudfoundry_user_provided_service" "logging" {
  name             = var.logging_service_name
  space            = data.cloudfoundry_space.space.id
  syslog_drain_url = "syslog-tls://${local.logstash_endpoint}"
}

resource "cloudfoundry_service_instance" "postgres" {
  name         = var.postgres_database_name
  space        = data.cloudfoundry_space.space.id
  service_plan = data.cloudfoundry_service.postgres.service_plans[var.postgres_database_service_plan]
  json_params  = jsonencode(local.restore_db_backup_params)
  timeouts {
    create = "60m"
    update = "60m"
  }
}

resource "azurerm_application_insights" "api_app_insights" {
  name                = var.api_app_insights_name
  resource_group_name = var.resource_group_name
  location            = "West Europe"
  application_type    = "web"

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "cloudfoundry_service_instance" "redis" {
  name         = var.redis_name
  space        = data.cloudfoundry_space.space.id
  service_plan = data.cloudfoundry_service.redis.service_plans[var.redis_service_plan]
}

resource "cloudfoundry_app" "api" {
  name                       = var.api_app_name
  space                      = data.cloudfoundry_space.space.id
  instances                  = var.api_instances
  memory                     = var.api_memory
  disk_quota                 = var.api_disk_quota
  docker_image               = var.api_docker_image
  strategy                   = "blue-green"
  environment                = local.api_app_config
  health_check_type          = "http"
  health_check_http_endpoint = "/health"

  dynamic "routes" {
    for_each = local.api_routes
    content {
      route = routes.value.id
    }
  }

  service_binding {
    service_instance = cloudfoundry_user_provided_service.logging.id
  }

  service_binding {
    service_instance = cloudfoundry_service_instance.postgres.id
  }

  service_binding {
    service_instance = cloudfoundry_service_instance.redis.id
  }

  depends_on = [
    azurerm_application_insights.api_app_insights
  ]
}
