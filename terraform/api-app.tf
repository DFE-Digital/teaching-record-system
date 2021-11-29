locals {
  api_app_config = {
    AppConfig = data.azurerm_key_vault_secret.secrets["APP-CONFIG"].value
  }

  logstash_endpoint = data.azurerm_key_vault_secret.secrets["LOGSTASH-ENDPOINT"].value
}

resource "cloudfoundry_route" "api_public" {
  domain   = data.cloudfoundry_domain.cloudapps.id
  hostname = var.api_app_name
  space    = data.cloudfoundry_space.space.id
}


resource "cloudfoundry_user_provided_service" "logging" {
  name             = "logit-ssl-drain"
  space            = data.cloudfoundry_space.space.id
  syslog_drain_url = "syslog-tls://${local.logstash_endpoint}"
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
}
