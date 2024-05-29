terraform {
  required_version = "1.5.0"

  backend "azurerm" {}

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.104.2"
    }

    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "2.30.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.6.2"
    }

    statuscake = {
      source  = "StatusCakeDev/statuscake"
      version = "2.2.0"
    }
  }
}
