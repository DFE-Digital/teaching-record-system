terraform {
  required_version = "1.5.0"

  backend "azurerm" {}

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.116.0"
    }

    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "2.32.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.6.2"
    }

    statuscake = {
      source  = "StatusCakeDev/statuscake"
      version = "2.2.2"
    }

    airbyte = {
      source  = "airbytehq/airbyte"
      version = "0.10.0"
    }
  }
}
