.DEFAULT_GOAL		:=help
SHELL				:=/bin/bash

TERRAFILE_VERSION=0.8
RG_TAGS={"Product" : "Teaching Record System"}
ARM_TEMPLATE_TAG=1.1.10
REGION=UK South
SERVICE_SHORT=trs

.PHONY: help
help: ## Show this help
	@grep -E '^[a-zA-Z\.\-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'
	## environments:
	## - AKS:  dev, test, pre-production, production

.PHONY: dv_review
dv_review: dev-cluster
	$(if $(CLUSTER), , $(error Missing environment variable "CLUSTER", Please specify a dev cluster name (eg 'cluster1')))
	$(if $(IMAGE), , $(error Missing environment variable "IMAGE", Please specify an image tag for your review app))
	$(if $(APP_NAME), , $(error Missing environment variable "APP_NAME", Please specify a pr number for your review app))
	$(eval DEPLOY_ENV=dv_review)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-development)
	$(eval RESOURCE_NAME_PREFIX=s189d01)
	$(eval ENV_SHORT=rv)
	$(eval ENV_TAG=dev)
	$(eval backend_key=-backend-config=key=$(APP_NAME).tfstate)
	$(eval export TF_VAR_cluster=$(CLUSTER))
	$(eval export TF_VAR_docker_image=$(IMAGE))
	$(eval export TF_VAR_app_name=$(APP_NAME))

.PHONY: dev
dev: test-cluster
	$(eval DEPLOY_ENV=dev)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-test)
	$(eval RESOURCE_NAME_PREFIX=s189t01)
	$(eval ENV_SHORT=dv)
	$(eval ENV_TAG=dev)

.PHONY: test
test: test-cluster
	$(eval DEPLOY_ENV=test)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-test)
	$(eval RESOURCE_NAME_PREFIX=s189t01)
	$(eval ENV_SHORT=ts)
	$(eval ENV_TAG=test)

.PHONY: pre-production
pre-production: test-cluster
	$(eval DEPLOY_ENV=pre-production)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-test)
	$(eval RESOURCE_NAME_PREFIX=s189t01)
	$(eval ENV_SHORT=pp)
	$(eval ENV_TAG=pre-prod)

.PHONY: production
production: production-cluster
	$(eval DEPLOY_ENV=production)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-production)
	$(eval RESOURCE_NAME_PREFIX=s189p01)
	$(eval ENV_SHORT=pd)
	$(eval ENV_TAG=prod)
	$(if $(or ${SKIP_CONFIRM}, ${CONFIRM_DEPLOY}), , $(error can only run with CONFIRM_DEPLOY))

.PHONY: domain
domain:
	$(eval DEPLOY_ENV=production)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-production)
	$(eval RESOURCE_NAME_PREFIX=s189p01)
	$(eval ENV_SHORT=pd)
	$(eval ENV_TAG=prod)

read-keyvault-config:
	$(eval KEY_VAULT_NAME=$(shell jq -r '.key_vault_name' terraform/aks/workspace_variables/$(DEPLOY_ENV).tfvars.json))
	$(eval KEY_VAULT_SECRET_NAME=$(shell jq -r '.key_vault_secret_name' terraform/aks/workspace_variables/$(DEPLOY_ENV).tfvars.json))

set-azure-account: ${environment}
	echo "Logging on to ${AZURE_SUBSCRIPTION}"
	az account set -s ${AZURE_SUBSCRIPTION}

set-azure-pd-subscription:
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-production)
	echo "setting subscription to"${AZURE_SUBSCRIPTION}
	az account set -s ${AZURE_SUBSCRIPTION}

ci:	## Run in automation environment
	$(eval AUTO_APPROVE=-auto-approve)
	$(eval SP_AUTH=true)
	$(eval CONFIRM_DEPLOY=true)
	$(eval SKIP_CONFIRM=true)

bin/terrafile: ## Install terrafile to manage terraform modules
	curl -sL https://github.com/coretech/terrafile/releases/download/v${TERRAFILE_VERSION}/terrafile_${TERRAFILE_VERSION}_$$(uname)_x86_64.tar.gz \
		| tar xz -C ./bin terrafile

terraform-init:
	$(eval export TF_VAR_service_name=$(SERVICE_SHORT))
	$(eval export TF_VAR_service_short_name=$(SERVICE_SHORT))
	$(eval export TF_VAR_environment_short_name=$(ENV_SHORT))
	$(eval export TF_VAR_azure_resource_prefix=$(RESOURCE_NAME_PREFIX))

	[[ "${SP_AUTH}" != "true" ]] && az account set -s $(AZURE_SUBSCRIPTION) || true
	terraform -chdir=terraform/aks init -upgrade -backend-config workspace_variables/${DEPLOY_ENV}.backend.tfvars  $(backend_key) -reconfigure

terraform-plan: terraform-init # make [env] terraform-plan init
	terraform -chdir=terraform/aks plan -var-file workspace_variables/${DEPLOY_ENV}.tfvars.json

terraform-apply: terraform-init
	terraform -chdir=terraform/aks apply -var-file workspace_variables/${DEPLOY_ENV}.tfvars.json ${AUTO_APPROVE}

terraform-destroy: terraform-init
	terraform -chdir=terraform/aks destroy -var-file workspace_variables/${DEPLOY_ENV}.tfvars.json ${AUTO_APPROVE}

deploy-azure-resources: set-azure-account # make dev deploy-azure-resources CONFIRM_DEPLOY=1
	$(if $(CONFIRM_DEPLOY), , $(error can only run with CONFIRM_DEPLOY))
	az deployment sub create --name "resourcedeploy-trs-$(shell date +%Y%m%d%H%M%S)" -l "${REGION}" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${RESOURCE_NAME_PREFIX}${SERVICE_SHORT}tfstate${ENV_SHORT}" "tfStorageContainerName=${SERVICE_SHORT}-tfstate" "dbBackupStorageAccountName=${AZURE_BACKUP_STORAGE_ACCOUNT_NAME}" "dbBackupStorageContainerName=${AZURE_BACKUP_STORAGE_CONTAINER_NAME}" "keyVaultNames=['${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-api-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-authz-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-inf-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-ui-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-worker-kv']"

validate-azure-resources: set-azure-account # make dev validate-azure-resources
	az deployment sub create --name "resourcedeploy-trs-$(shell date +%Y%m%d%H%M%S)"  -l  "${REGION}" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${RESOURCE_NAME_PREFIX}${SERVICE_SHORT}tfstate${ENV_SHORT}" "tfStorageContainerName=${SERVICE_SHORT}-tfstate" "dbBackupStorageAccountName=${AZURE_BACKUP_STORAGE_ACCOUNT_NAME}" "dbBackupStorageContainerName=${AZURE_BACKUP_STORAGE_CONTAINER_NAME}" "keyVaultNames=['${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-api-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-authz-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-inf-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-ui-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-worker-kv']" --what-if

domains-init: bin/terrafile set-azure-pd-subscription ## make [env] domains-init -  terraform init for environment dns/afd resources
	./bin/terrafile -p terraform/domains/environment_domains/vendor/modules -f terraform/domains/environment_domains/config/${DEPLOY_ENV}_Terrafile
	terraform -chdir=terraform/domains/environment_domains init -reconfigure -upgrade -backend-config=config/${DEPLOY_ENV}_backend.tfvars

domains-plan: domains-init ## terraform plan for environment dns/afd resources
	terraform -chdir=terraform/domains/environment_domains plan -var-file config/${DEPLOY_ENV}.tfvars.json

domains-apply: domains-init ## terraform apply for environment dns/afd resources, needs CONFIRM_DEPLOY=1 for production
	terraform -chdir=terraform/domains/environment_domains apply -var-file config/${DEPLOY_ENV}.tfvars.json

domains-infra-init: bin/terrafile set-azure-pd-subscription ## make domain domains-infra-init - terraform init for dns/afd core resources, eg Main FrontDoor resource
	./bin/terrafile -p terraform/domains/infrastructure/vendor/modules -f terraform/domains/infrastructure/config/trs_Terrafile
	terraform -chdir=terraform/domains/infrastructure init -reconfigure -upgrade

domains-infra-plan: domains-infra-init ## terraform plan for dns core resources
	terraform -chdir=terraform/domains/infrastructure plan -var-file config/trs.tfvars.json

domains-infra-apply: domains-infra-init ## terraform apply for dns core resources
	terraform -chdir=terraform/domains/infrastructure apply -var-file config/trs.tfvars.json

domain-azure-resources: set-azure-account # make domain domain-azure-resources CONFIRM_DEPLOY=1, creates core DNA/AKS
	$(if $(CONFIRM_DEPLOY), , $(error can only run with CONFIRM_DEPLOY))
	az deployment sub create -l "UK South" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${RESOURCE_NAME_PREFIX}-trsdomains-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${RESOURCE_NAME_PREFIX}trsdomainstf" "tfStorageContainerName=trsdomains-tf"  "keyVaultName=${RESOURCE_NAME_PREFIX}-trsdomain-kv"

.PHONY: install-konduit
install-konduit: ## Install the konduit script, for accessing backend services
	[ ! -f bin/konduit.sh ] \
		&& curl -s https://raw.githubusercontent.com/DFE-Digital/teacher-services-cloud/master/scripts/konduit.sh -o bin/konduit.sh \
		&& chmod +x bin/konduit.sh \
		|| true

test-cluster:
	$(eval CLUSTER_RESOURCE_GROUP_NAME=s189t01-tsc-ts-rg)
	$(eval CLUSTER_NAME=s189t01-tsc-test-aks)

production-cluster:
	$(eval CLUSTER_RESOURCE_GROUP_NAME=s189p01-tsc-pd-rg)
	$(eval CLUSTER_NAME=s189p01-tsc-production-aks)

get-cluster-credentials: set-azure-account
	az aks get-credentials --overwrite-existing -g ${CLUSTER_RESOURCE_GROUP_NAME} -n ${CLUSTER_NAME}
	kubelogin convert-kubeconfig -l $(if ${GITHUB_ACTIONS},spn,azurecli)

dev-cluster:
	$(eval CLUSTER_RESOURCE_GROUP_NAME=s189d01-tsc-dv-rg)
	$(eval CLUSTER_NAME=s189d01-tsc-${CLUSTER}-aks)
