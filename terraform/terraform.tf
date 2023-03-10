terraform {
  required_version = "1.0.10"

  backend "azurerm" {
    container_name = "dqtapi-tfstate"
  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.47.0"
    }

    cloudfoundry = {
      source  = "cloudfoundry-community/cloudfoundry"
      version = "0.50.4"
    }

    statuscake = {
      source  = "StatusCakeDev/statuscake"
      version = "2.0.6"
    }
  }
}
