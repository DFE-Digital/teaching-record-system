module "statuscake" {
  count = var.enable_monitoring ? 1 : 0

  source = "git::https://github.com/DFE-Digital/terraform-modules.git//monitoring/statuscake?ref=8c6c2e0dbb6a5e5a9c78db3a55910a8cdd9250d8"

  uptime_urls = concat(
    [
      module.api_application.probe_url,
      module.ui_application.probe_url
    ],
    var.statuscake_extra_urls
  )

  contact_groups = [288912]

  confirmation = 0
}
