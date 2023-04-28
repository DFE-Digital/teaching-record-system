resource "azurerm_mssql_server" "reporting_server" {
  count                        = var.reporting_db_server_name != "" ? 1 : 0
  name                         = var.reporting_db_server_name
  resource_group_name          = var.resource_group_name
  location                     = "West Europe"
  version                      = "12.0"
  administrator_login          = yamldecode(data.azurerm_key_vault_secret.secrets["REPORTING-DB"].value)["USERNAME"]
  administrator_login_password = yamldecode(data.azurerm_key_vault_secret.secrets["REPORTING-DB"].value)["PASSWORD"]

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_mssql_firewall_rule" "reporting_server_paas_access" {
  count            = length(azurerm_mssql_server.reporting_server) == 1 ? length(local.paas_egress_ips) : 0
  name             = "PAAS${count.index}"
  server_id        = azurerm_mssql_server.reporting_server[0].id
  start_ip_address = local.paas_egress_ips[count.index]
  end_ip_address   = local.paas_egress_ips[count.index]
}

resource "azurerm_mssql_database" "reporting_db" {
  count       = length(azurerm_mssql_server.reporting_server)
  name        = var.reporting_db_name
  server_id   = azurerm_mssql_server.reporting_server[0].id
  collation   = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb = 10
  sku_name    = "S0"

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}
