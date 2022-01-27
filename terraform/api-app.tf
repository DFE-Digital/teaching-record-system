locals {
  api_app_config = {
    AppConfig       = data.azurerm_key_vault_secret.secrets["APP-CONFIG"].value,
    AppVersion      = var.api_app_version,
    PaasEnvironment = var.environment_name
  }

  logstash_endpoint = data.azurerm_key_vault_secret.secrets["LOGSTASH-ENDPOINT"].value
}

resource "cloudfoundry_route" "api_public" {
  domain   = data.cloudfoundry_domain.cloudapps.id
  hostname = var.api_app_name
  space    = data.cloudfoundry_space.space.id
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
}

resource "null_resource" "migrations" {
  triggers = {
    migrations = "${sha1(file(var.migrations_file))}"
  }

  provisioner "local-exec" {
    command = "cf target -s ${var.paas_space} && cf conduit ${cloudfoundry_service_instance.postgres.name} -- psql -f ${var.migrations_file}"
  }

  depends_on = [
    cloudfoundry_service_instance.postgres
  ]
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

  routes {
    route = cloudfoundry_route.api_public.id
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
    null_resource.migrations
  ]
}
