variable "zone" {
  type        = string
  description = "Name of DNS zone"
  default     = "teacher-qualifications-api.education.gov.uk"
}

variable "front_door_name" {
  type        = string
  description = "Name of Azure Front Door"
  default     = "s189p01-trsdomains-fd"
}

variable "resource_group_name" {
  type        = string
  description = "Name of resouce group name"
  default     = "s189p01-trsdomains-rg"
}

variable "domains" {
  description = "List of domains record names"
}

variable "environment_tag" {
  type        = string
  description = "Environment"
}

variable "environment_short" {
  type        = string
  description = "Short name for environment"
}

variable "origin_hostname" {
  type        = string
  description = "Origin endpoint url"
}

variable "null_host_header" {
  type        = bool
  description = "origin_host_header for the azurerm_cdn_frontdoor_origin"

}

variable "hosted_zone" {
  type    = map(any)
  default = {}
}

locals {
  hostname = "${var.domains[0]}.${var.zone}"
}
