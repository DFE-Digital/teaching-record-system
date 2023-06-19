output "api_fqdn" {
  value = element(
    concat(
      [for h in values(cloudfoundry_route.api_education) : "${h.hostname}.${data.cloudfoundry_domain.education_gov_uk.name}"],
    ["${cloudfoundry_route.api_public.hostname}.${data.cloudfoundry_domain.cloudapps.name}"]),
  0)
}
