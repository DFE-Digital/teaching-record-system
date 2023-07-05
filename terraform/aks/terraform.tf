terraform {
  backend "azurerm" {}

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.57.0"
    }

    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "2.20.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.5.1"
    }

    statuscake = {
      source  = "StatusCakeDev/statuscake"
      version = "2.1.0"
    }
  }
}
