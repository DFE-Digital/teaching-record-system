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

variable "azure_sp_credentials_json" {
  type    = string
  default = null
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

variable "run_recurring_jobs" {
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

locals {
  app_name_suffix = var.app_name == null ? var.environment_name : var.app_name
}
