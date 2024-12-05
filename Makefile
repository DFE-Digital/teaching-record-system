.DEFAULT_GOAL		:=help
SHELL				:=/bin/bash

RG_TAGS={"Product" : "Teaching Record System"}
ARM_TEMPLATE_TAG=1.1.10
REGION=UK South
SERVICE_SHORT=trs

.PHONY: help
help: ## Show this help
	@grep -E '^[a-zA-Z\.\-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'
	## environments:
	## - AKS: dev, test, pre-production, production

.PHONY: dv_review
dv_review: dev-cluster
	$(if $(CLUSTER), , $(error Missing environment variable "CLUSTER", Please specify a dev cluster name (eg 'cluster1')))
	$(if $(IMAGE), , $(error Missing environment variable "IMAGE", Please specify an image tag for your review app))
	$(if $(APP_NAME), , $(error Missing environment variable "APP_NAME", Please specify a pr number for your review app))
	$(eval include global_config/dv_review.sh)
	$(eval backend_key=-backend-config=key=$(APP_NAME).tfstate)
	$(eval export TF_VAR_cluster=$(CLUSTER))
	$(eval export TF_VAR_docker_image=$(IMAGE))
	$(eval export TF_VAR_app_name=$(APP_NAME))

.PHONY: dev
dev: test-cluster
	$(eval include global_config/dev.sh)

.PHONY: test
test: test-cluster
	$(eval include global_config/test.sh)

.PHONY: pre-production
pre-production: test-cluster
	$(eval include global_config/pre-production.sh)

.PHONY: production
production: production-cluster
	$(eval include global_config/production.sh)
	$(if $(or ${SKIP_CONFIRM}, ${CONFIRM_DEPLOY}), , $(error can only run with CONFIRM_DEPLOY))

.PHONY: domains
domains:
	$(eval include global_config/domains.sh)

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

vendor-modules:
	rm -rf terraform/aks/vendor/modules/aks
	git -c advice.detachedHead=false clone --depth=1 --single-branch --branch ${TERRAFORM_MODULES_TAG} https://github.com/DFE-Digital/terraform-modules.git terraform/aks/vendor/modules/aks

terraform-init: vendor-modules
	$(eval export TF_VAR_service_name=$(SERVICE_SHORT))
	$(eval export TF_VAR_service_short_name=$(SERVICE_SHORT))
	$(eval export TF_VAR_config=${CONFIG})
	$(eval export TF_VAR_environment_short_name=$(CONFIG_SHORT))
	$(eval export TF_VAR_azure_resource_prefix=$(AZURE_RESOURCE_PREFIX))

	[[ "${SP_AUTH}" != "true" ]] && az account set -s $(AZURE_SUBSCRIPTION) || true

	terraform -chdir=terraform/aks init -upgrade -backend-config config/${CONFIG}.backend.tfvars $(backend_key) -reconfigure

terraform-plan: terraform-init # make [env] terraform-plan init
	terraform -chdir=terraform/aks plan -var-file config/${CONFIG}.tfvars.json

terraform-apply: terraform-init
	terraform -chdir=terraform/aks apply -var-file config/${CONFIG}.tfvars.json ${AUTO_APPROVE}

terraform-destroy: terraform-init
	terraform -chdir=terraform/aks destroy -var-file config/${CONFIG}.tfvars.json ${AUTO_APPROVE}

deploy-azure-resources: set-azure-account # make dev deploy-azure-resources CONFIRM_DEPLOY=1
	$(if $(CONFIRM_DEPLOY), , $(error can only run with CONFIRM_DEPLOY))
	az deployment sub create --name "resourcedeploy-trs-$(shell date +%Y%m%d%H%M%S)" -l "${REGION}" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${AZURE_RESOURCE_PREFIX}${SERVICE_SHORT}tfstate${CONFIG_SHORT}" "tfStorageContainerName=${SERVICE_SHORT}-tfstate" "dbBackupStorageAccountName=${AZURE_BACKUP_STORAGE_ACCOUNT_NAME}" "dbBackupStorageContainerName=${AZURE_BACKUP_STORAGE_CONTAINER_NAME}" "keyVaultNames=['${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-api-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-authz-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-inf-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-ui-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-worker-kv']"

validate-azure-resources: set-azure-account # make dev validate-azure-resources
	az deployment sub create --name "resourcedeploy-trs-$(shell date +%Y%m%d%H%M%S)" -l "${REGION}" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${AZURE_RESOURCE_PREFIX}${SERVICE_SHORT}tfstate${CONFIG_SHORT}" "tfStorageContainerName=${SERVICE_SHORT}-tfstate" "dbBackupStorageAccountName=${AZURE_BACKUP_STORAGE_ACCOUNT_NAME}" "dbBackupStorageContainerName=${AZURE_BACKUP_STORAGE_CONTAINER_NAME}" "keyVaultNames=['${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-api-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-authz-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-inf-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-ui-kv', '${AZURE_RESOURCE_PREFIX}-${SERVICE_SHORT}-${CONFIG_SHORT}-worker-kv']" --what-if

domains-init: domains set-azure-pd-subscription ## make [env] domains-init - terraform init for environment dns/afd resources
	rm -rf terraform/domains/environment_domains/vendor/modules/domains
	git -c advice.detachedHead=false clone --depth=1 --single-branch --branch ${TERRAFORM_MODULES_TAG} https://github.com/DFE-Digital/terraform-modules.git terraform/domains/environment_domains/vendor/modules/domains

	terraform -chdir=terraform/domains/environment_domains init -reconfigure -upgrade -backend-config=config/${CONFIG}_backend.tfvars

domains-plan: domains-init ## terraform plan for environment dns/afd resources
	terraform -chdir=terraform/domains/environment_domains plan -var-file config/${CONFIG}.tfvars.json

domains-apply: domains-init ## terraform apply for environment dns/afd resources, needs CONFIRM_DEPLOY=1 for production
	terraform -chdir=terraform/domains/environment_domains apply -var-file config/${CONFIG}.tfvars.json

domains-infra-init: domains set-azure-pd-subscription ## make domains-infra-init - terraform init for dns/afd core resources, eg Main FrontDoor resource
	rm -rf terraform/domains/infrastructure/vendor/modules/domains
	git -c advice.detachedHead=false clone --depth=1 --single-branch --branch ${TERRAFORM_MODULES_TAG} https://github.com/DFE-Digital/terraform-modules.git terraform/domains/infrastructure/vendor/modules/domains

	terraform -chdir=terraform/domains/infrastructure init -reconfigure -upgrade

domains-infra-plan: domains-infra-init ## terraform plan for dns core resources
	terraform -chdir=terraform/domains/infrastructure plan -var-file config/trs.tfvars.json

domains-infra-apply: domains-infra-init ## terraform apply for dns core resources
	terraform -chdir=terraform/domains/infrastructure apply -var-file config/trs.tfvars.json

domains-azure-resources: domains set-azure-account # make domains-azure-resources CONFIRM_DEPLOY=1, creates core DNA/AKS
	$(if $(CONFIRM_DEPLOY), , $(error can only run with CONFIRM_DEPLOY))
	az deployment sub create -l "UK South" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${AZURE_RESOURCE_PREFIX}-trsdomains-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${AZURE_RESOURCE_PREFIX}trsdomainstf" "tfStorageContainerName=trsdomains-tf" "keyVaultName=${AZURE_RESOURCE_PREFIX}-trsdomain-kv"

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
