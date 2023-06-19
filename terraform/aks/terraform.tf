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
  }
}
