locals {
  reporting_db_username          = var.deploy_dqt_reporting_server ? "u${random_string.reporting_server_username[0].result}" : null
  reporting_db_password          = var.deploy_dqt_reporting_server ? random_string.reporting_server_password[0].result : null
  reporting_db_connection_string = var.deploy_dqt_reporting_server ? "Data Source=tcp:${azurerm_mssql_server.reporting_server[0].fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.reporting_db[0].name};Persist Security Info=False;User ID=${local.reporting_db_username};Password=${local.reporting_db_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" : module.infrastructure_secrets.map.DQT-REPORTING-CONNECTION-STRING
}

resource "random_string" "reporting_server_username" {
  count   = var.deploy_dqt_reporting_server ? 1 : 0
  length  = 15
  special = false
  upper   = false
}

resource "random_string" "reporting_server_password" {
  count   = var.deploy_dqt_reporting_server ? 1 : 0
  length  = 32
  special = true
}

resource "azurerm_mssql_server" "reporting_server" {
  count                        = var.deploy_dqt_reporting_server ? 1 : 0
  name                         = "${var.azure_resource_prefix}${var.service_short_name}${var.environment_short_name}repsqlsvr"
  resource_group_name          = var.resource_group_name
  location                     = var.region
  version                      = "12.0"
  administrator_login          = local.reporting_db_username
  administrator_login_password = local.reporting_db_password

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_mssql_firewall_rule" "reporting_server_azure_access" {
  count            = var.deploy_dqt_reporting_server ? 1 : 0
  name             = "Allow Azure"
  server_id        = azurerm_mssql_server.reporting_server[0].id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "reporting_db" {
  count       = length(azurerm_mssql_server.reporting_server)
  name        = "${var.azure_resource_prefix}${var.service_short_name}${var.environment_short_name}repsqldb"
  server_id   = azurerm_mssql_server.reporting_server[count.index].id
  collation   = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb = 30
  sku_name    = "S0"

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "kubernetes_job" "reporting_migrations" {
  metadata {
    name      = "${var.service_name}-${var.environment_name}-reporting-migrations"
    namespace = var.namespace
  }

  spec {
    template {
      metadata {}
      spec {
        container {
          name    = "cli"
          image   = var.docker_image
          command = ["trscli"]
          args    = ["migrate-reporting-db", "--connection-string", "$(CONNECTION_STRING)"]

          env {
            name  = "CONNECTION_STRING"
            value = local.reporting_db_connection_string
          }
        }

        restart_policy = "Never"
      }
    }

    backoff_limit = 1
  }

  wait_for_completion = true
}
