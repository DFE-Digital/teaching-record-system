module "statuscake" {
  count = var.enable_monitoring ? 1 : 0

  source = "git::https://github.com/DFE-Digital/terraform-modules.git//monitoring/statuscake?ref=stable"

  uptime_urls = concat(
    [
      module.api_application.probe_url,
      module.ui_application.probe_url
    ],
    var.statuscake_extra_urls
  )

  contact_groups = [288912]
}
