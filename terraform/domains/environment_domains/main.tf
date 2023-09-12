module "domains" {
  source              = "./vendor/modules/domains//domains/environment_domains"
  zone                = var.zone
  front_door_name     = var.front_door_name
  resource_group_name = var.resource_group_name
  domains             = var.domains
  environment         = var.environment_short
  host_name           = var.origin_hostname
  null_host_header    = var.null_host_header
}
module "records" {
  source      = "./vendor/modules/domains//dns/records"
  hosted_zone = var.hosted_zone

}
