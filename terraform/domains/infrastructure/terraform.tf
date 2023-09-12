terraform {
  required_version = "1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.53.0"
    }
  }

  backend "azurerm" {
    container_name       = "trsdomains-tf"
    resource_group_name  = "s189p01-trsdomains-rg"
    storage_account_name = "s189p01trsdomainstf"
    key                  = "trsdomains.tfstate"
  }
}

provider "azurerm" {
  features {}

  skip_provider_registration = true
}
