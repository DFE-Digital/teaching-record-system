module "statuscake" {
  count = var.enable_monitoring ? 1 : 0

  source = "git::https://github.com/DFE-Digital/terraform-modules.git//monitoring/statuscake?ref=testing"

  uptime_urls = compact([module.api_application.probe_url, module.ui_application.probe_url])

  contact_groups = [288912]
}
