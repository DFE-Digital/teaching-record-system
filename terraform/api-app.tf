locals {
  api_app_config = {
    AppConfig = data.azurerm_key_vault_secret.secrets["APP-CONFIG"].value
  }
}

resource "cloudfoundry_route" "api_public" {
  domain   = data.cloudfoundry_domain.cloudapps.id
  hostname = var.api_app_name
  space    = data.cloudfoundry_space.space.id
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
}
