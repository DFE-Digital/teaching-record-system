output "api_fqdn" {
  value = "${cloudfoundry_route.api_public.hostname}.${data.cloudfoundry_domain.cloudapps.name}"
}
