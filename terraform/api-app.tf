locals {
  appconfig_json                  = data.azurerm_key_vault_secret.secrets["APP-CONFIG"].value
  logstash_endpoint               = data.azurerm_key_vault_secret.secrets["LOGSTASH-ENDPOINT"].value
  distributed_lock_container_name = "locks"
  pg_credentials                  = cloudfoundry_service_key.postgres-key.credentials
  pg_connection_string            = "Host=${local.pg_credentials.host};Database=${local.pg_credentials.name};Username=${local.pg_credentials.username};Password='${local.pg_credentials.password}';Port=${local.pg_credentials.port};SslMode=Require;TrustServerCertificate=true"
  redis_credentials               = cloudfoundry_service_key.redis-key.credentials
  redis_connection_string         = "${local.redis_credentials.host}:${local.redis_credentials.port},password=${local.redis_credentials.password},ssl=True"
  dqt_reporting_connection_string = length(azurerm_mssql_server.reporting_server) == 1 ? "Data Source=tcp:${azurerm_mssql_server.reporting_server[0].fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.reporting_db[0].name};Persist Security Info=False;User ID=${azurerm_mssql_server.reporting_server[0].administrator_login};Password=${yamldecode(data.azurerm_key_vault_secret.secrets["REPORTING-DB"].value)["PASSWORD"]};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" : ""

  api_app_config = {
    AppConfig = jsonencode(merge(
      local.dqt_reporting_connection_string != "" ? { "DqtReporting:ReportingDbConnectionString" = local.dqt_reporting_connection_string } : {},
      jsondecode(local.appconfig_json),
      {
        "ApplicationInsights:ConnectionString" = azurerm_application_insights.api_app_insights.connection_string,
        "AppVersion"                           = var.api_app_version,
        "ConnectionStrings:DefaultConnection"  = local.pg_connection_string,
        "ConnectionStrings:Redis"              = local.redis_connection_string,
        "DistributedLockContainerName"         = local.distributed_lock_container_name,
        "PlatformEnvironment"                  = var.environment_name,
        "StorageConnectionString"              = "DefaultEndpointsProtocol=https;AccountName=${azurerm_storage_account.app-storage.name};AccountKey=${azurerm_storage_account.app-storage.primary_access_key}",
        "Platform"                             = "PAAS"
      }
    ))
  }
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

resource "cloudfoundry_service_key" "postgres-key" {
  name             = "${var.postgres_database_name}-key"
  service_instance = cloudfoundry_service_instance.postgres.id
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

resource "cloudfoundry_service_key" "redis-key" {
  name             = "${var.redis_name}-key"
  service_instance = cloudfoundry_service_instance.redis.id
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

  depends_on = [
    azurerm_application_insights.api_app_insights
  ]
}
