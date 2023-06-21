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

variable "deploy_azure_backing_services" {
  type    = string
  default = true
}

variable "api_docker_image" {
  type = string
}

variable "ui_docker_image" {
  type = string
}
