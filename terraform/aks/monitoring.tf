module "statuscake" {
  count = var.enable_monitoring ? 1 : 0

  source = "./vendor/modules/aks//monitoring/statuscake"

  uptime_urls = concat(
    [
      module.api_application.probe_url,
      module.ui_application.probe_url
    ],
    var.statuscake_extra_urls
  )

  ssl_urls = var.ssl_urls

  contact_groups = [288912, 282453]
}
