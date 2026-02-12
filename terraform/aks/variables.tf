variable "service_name" {
  type = string
}

variable "service_short_name" {
  type = string
}

variable "environment_name" {
  type = string
}

variable "environment_short_name" {
  type = string
}

variable "cluster" {
  type = string
}

variable "namespace" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "azure_resource_prefix" {
  type = string
}

variable "region" {
  type    = string
  default = "UK South"
}

variable "enable_monitoring" {
  type    = bool
  default = true
}

variable "enable_logit" { default = false }

variable "enable_prometheus_monitoring" {
  type    = bool
  default = false
}

variable "deploy_azure_backing_services" {
  type    = string
  default = true
}

variable "docker_image" {
  type = string
}

variable "deploy_dqt_reporting_server" {
  type    = bool
  default = false
}

variable "run_dqt_reporting_service" {
  type = bool
}

variable "api_replicas" {
  type    = number
  default = 1
}

variable "api_max_memory" {
  type    = string
  default = "1Gi"
}

variable "authz_replicas" {
  type    = number
  default = 1
}

variable "authz_max_memory" {
  type    = string
  default = "1Gi"
}

variable "ui_replicas" {
  type    = number
  default = 1
}

variable "worker_replicas" {
  type    = number
  default = 1
}

variable "worker_max_memory" {
  type    = string
  default = "1Gi"
}

variable "postgres_flexible_server_sku" {
  type    = string
  default = "B_Standard_B1ms"
}

variable "postgres_enable_high_availability" {
  type    = bool
  default = false
}

variable "postgres_server_version" {
  type    = string
  default = "14"
}

variable "postgres_azure_storage_mb" {
  type    = number
  default = 32768
}

variable "redis_capacity" {
  type    = number
  default = 1
}

variable "redis_family" {
  type    = string
  default = "C"
}

variable "redis_sku_name" {
  type    = string
  default = "Standard"
}

# StatusCake variables
variable "ssl_urls" {
  type    = list(string)
  default = []
}

variable "statuscake_extra_urls" {
  type        = list(string)
  description = "List of extra URLs for StatusCake, on top of the internal teacherservices.cloud ones"
  default     = []
}

variable "app_name" { default = null }

variable "app_name_suffix" { default = null }

variable "enable_dfe_analytics_federated_auth" {
  description = "Create the resources in Google cloud for federated authentication and enable in application"
  default     = false
}

variable "run_as_non_root" {
  type        = bool
  default     = true
  description = "Whether to enforce that containers must run as non-root user"
}

# pg_airbyte_enabled used in the postgres module
variable "pg_airbyte_enabled" { default = false }

locals {
  app_name_suffix = var.app_name == null ? var.environment_name : var.app_name

  federated_auth_configmap = var.enable_dfe_analytics_federated_auth ? {
    DfeAnalytics__Environment = var.environment_name
    DfeAnalytics__TableId     = module.dfe_analytics[0].bigquery_table_name
    DfeAnalytics__DatasetId   = module.dfe_analytics[0].bigquery_dataset
    DfeAnalytics__ProjectId   = module.dfe_analytics[0].bigquery_project_id
  } : {}

  federated_auth_secrets = var.enable_dfe_analytics_federated_auth ? {
    DfeAnalytics__CredentialsJson = module.dfe_analytics[0].google_cloud_credentials
  } : {}
}
