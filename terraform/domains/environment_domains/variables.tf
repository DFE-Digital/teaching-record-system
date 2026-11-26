variable "hosted_zone" {
  type    = map(any)
  default = {}
}

variable "rate_limit" {
  type = list(object({
    action         = optional(string)
    agent          = optional(string)
    priority       = optional(number)
    duration       = optional(number)
    limit          = optional(number)
    match_variable = optional(string)
    selector       = optional(string)
    operator       = optional(string)
    match_values   = optional(string)
  }))
  default = null
}

variable "allow_aks" {
  type        = bool
  default     = false
  description = "allow all ingress from the aks cluster when a rate limit rule is active"
}
