.DEFAULT_GOAL		:=help
SHELL				:=/bin/bash

TERRAFILE_VERSION=0.8
RG_TAGS={"Product" : "Teaching Record System"}
ARM_TEMPLATE_TAG=1.1.10

.PHONY: help
help: ## Show this help
	@grep -E '^[a-zA-Z\.\-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'
	## environments:
	## - Paas: dev, test, pre-production, production
	## - AKS:  dev_aks, test_aks, pre-production_aks, production_aks

.PHONY: paas
paas:
	$(eval PLATFORM=paas)
	$(eval REGION=West Europe)
	$(eval SERVICE_SHORT=dqtapi)

.PHONY: aks
aks:
	$(eval PLATFORM=aks)
	$(eval REGION=UK South)
	$(eval SERVICE_SHORT=trs)

.PHONY: dev
dev: paas
	$(eval DEPLOY_ENV=dev)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-development)
	$(eval RESOURCE_NAME_PREFIX=s165d01)
	$(eval ENV_SHORT=dv)
	$(eval ENV_TAG=dev)

.PHONY: test
test: paas
	$(eval DEPLOY_ENV=test)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-test)
	$(eval RESOURCE_NAME_PREFIX=s165t01)
	$(eval ENV_SHORT=ts)
	$(eval ENV_TAG=test)

.PHONY: pre-production
pre-production: paas
	$(eval DEPLOY_ENV=pre-production)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-test)
	$(eval RESOURCE_NAME_PREFIX=s165t01)
	$(eval ENV_SHORT=pp)
	$(eval ENV_TAG=pre-prod)

.PHONY: production
production: paas
	$(eval DEPLOY_ENV=production)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-production)
	$(eval RESOURCE_NAME_PREFIX=s165p01)
	$(eval ENV_SHORT=pd)
	$(eval ENV_TAG=prod)
	$(eval AZURE_BACKUP_STORAGE_ACCOUNT_NAME=s165p01dqtapidbbackup)
	$(eval AZURE_BACKUP_STORAGE_CONTAINER_NAME=dqt-api)

.PHONY: dev_aks
dev_aks: aks test-cluster
	$(eval DEPLOY_ENV=dev)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-test)
	$(eval RESOURCE_NAME_PREFIX=s189t01)
	$(eval ENV_SHORT=dv)
	$(eval ENV_TAG=dev)

.PHONY: test_aks
test_aks: aks test-cluster
	$(eval DEPLOY_ENV=test)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-test)
	$(eval RESOURCE_NAME_PREFIX=s189t01)
	$(eval ENV_SHORT=ts)
	$(eval ENV_TAG=test)

.PHONY: pre-production_aks
pre-production_aks: aks test-cluster
	$(eval DEPLOY_ENV=pre-production)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-test)
	$(eval RESOURCE_NAME_PREFIX=s189t01)
	$(eval ENV_SHORT=pp)
	$(eval ENV_TAG=pre-prod)

.PHONY: production_aks
production_aks: aks production-cluster
	$(eval DEPLOY_ENV=production)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-production)
	$(eval RESOURCE_NAME_PREFIX=s189p01)
	$(eval ENV_SHORT=pd)
	$(eval ENV_TAG=prod)
	$(if $(or ${SKIP_CONFIRM}, ${CONFIRM_DEPLOY}), , $(error can only run with CONFIRM_DEPLOY))

.PHONY: domain
domain:
	$(eval DEPLOY_ENV=production)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-production)
	$(eval RESOURCE_NAME_PREFIX=s165p01)
	$(eval ENV_SHORT=pd)
	$(eval ENV_TAG=prod)

.PHONY: aks_domain
aks_domain:
	$(eval DEPLOY_ENV=production)
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-production)
	$(eval RESOURCE_NAME_PREFIX=s189p01)
	$(eval ENV_SHORT=pd)
	$(eval ENV_TAG=prod)

read-keyvault-config:
	$(eval KEY_VAULT_NAME=$(shell jq -r '.key_vault_name' terraform/$(PLATFORM)/workspace_variables/$(DEPLOY_ENV).tfvars.json))
	$(eval KEY_VAULT_SECRET_NAME=$(shell jq -r '.key_vault_secret_name' terraform/$(PLATFORM)/workspace_variables/$(DEPLOY_ENV).tfvars.json))

read-deployment-config:
	$(eval SPACE=$(shell jq -r '.paas_space' terraform/$(DEPLOY_ENV).tfvars.json))
	$(eval POSTGRES_DATABASE_NAME=$(shell jq -r '.postgres_database_name' terraform/$(DEPLOY_ENV).tfvars.json))
	$(eval API_APP_NAME=$(shell jq -r '.api_app_name' terraform/$(DEPLOY_ENV).tfvars.json))

set-azure-account: ${environment}
	echo "Logging on to ${AZURE_SUBSCRIPTION}"
	az account set -s ${AZURE_SUBSCRIPTION}

set-azure-pd-subscription:
	$(eval AZURE_SUBSCRIPTION=s189-teacher-services-cloud-production)
	echo "setting subscription to"${AZURE_SUBSCRIPTION}
	az account set -s ${AZURE_SUBSCRIPTION}

ci:	## Run in automation environment
	$(eval DISABLE_PASSCODE=true)
	$(eval AUTO_APPROVE=-auto-approve)
	$(eval SP_AUTH=true)
	$(eval CONFIRM_DEPLOY=true)
	$(eval SKIP_CONFIRM=true)

bin/terrafile: ## Install terrafile to manage terraform modules
	curl -sL https://github.com/coretech/terrafile/releases/download/v${TERRAFILE_VERSION}/terrafile_${TERRAFILE_VERSION}_$$(uname)_x86_64.tar.gz \
		| tar xz -C ./bin terrafile

.PHONY: install-fetch-config
install-fetch-config: ## Install the fetch-config script, for viewing/editing secrets in Azure Key Vault
	[ ! -f bin/fetch_config.rb ] \
		&& curl -s https://raw.githubusercontent.com/DFE-Digital/bat-platform-building-blocks/master/scripts/fetch_config/fetch_config.rb -o bin/fetch_config.rb \
		&& chmod +x bin/fetch_config.rb \
		|| true

edit-infra-secrets: read-keyvault-config install-fetch-config set-azure-account
	bin/fetch_config.rb -s azure-key-vault-secret:${KEY_VAULT_NAME}/${KEY_VAULT_SECRET_NAME} \
		-e -d azure-key-vault-secret:${KEY_VAULT_NAME}/${KEY_VAULT_SECRET_NAME} -f yaml -c

print-infra-secrets: read-keyvault-config install-fetch-config set-azure-account
	bin/fetch_config.rb -s azure-key-vault-secret:${KEY_VAULT_NAME}/${KEY_VAULT_SECRET_NAME} -f yaml

validate-infra-secrets: read-keyvault-config install-fetch-config set-azure-account
	bin/fetch_config.rb -s azure-key-vault-secret:${KEY_VAULT_NAME}/${KEY_VAULT_SECRET_NAME} -d quiet \
		&& echo Data in ${KEY_VAULT_NAME}/${KEY_VAULT_SECRET_NAME} looks valid

.PHONY: set-space-developer
set-space-developer: read-deployment-config ## make dev set-space-developer USER_ID=first.last@digital.education.gov.uk
	$(if $(USER_ID), , $(error Missing environment variable "USER_ID", USER_ID required for this command to run))
	cf set-space-role ${USER_ID} dfe ${SPACE} SpaceDeveloper

.PHONY: unset-space-developer
unset-space-developer: read-deployment-config ## make dev unset-space-developer USER_ID=first.last@digital.education.gov.uk
	$(if $(USER_ID), , $(error Missing environment variable "USER_ID", USER_ID required for this command to run))
	cf unset-space-role ${USER_ID} dfe ${SPACE} SpaceDeveloper

stop-app: read-deployment-config ## Stops api app, make dev stop-app CONFIRM_STOP=1
	$(if $(CONFIRM_STOP), , $(error stop-app can only run with CONFIRM_STOP))
	cf target -s ${SPACE}
	cf stop ${API_APP_NAME}

get-postgres-instance-guid: read-deployment-config ## Gets the postgres service instance's guid
	cf target -s ${SPACE} > /dev/null
	cf service ${POSTGRES_DATABASE_NAME} --guid
	$(eval DB_INSTANCE_GUID=$(shell cf service ${POSTGRES_DATABASE_NAME} --guid))

rename-postgres-service: read-deployment-config ## make dev rename-postgres-service NEW_NAME_SUFFIX=old CONFIRM_RENAME
	$(if $(CONFIRM_RENAME), , $(error can only run with CONFIRM_RENAME))
	$(if $(NEW_NAME_SUFFIX), , $(error NEW_NAME_SUFFIX is required))
	cf target -s ${SPACE} > /dev/null
	cf rename-service  ${POSTGRES_DATABASE_NAME} ${POSTGRES_DATABASE_NAME}-$(NEW_NAME_SUFFIX)

remove-postgres-tf-state: terraform-init ## make dev remove-postgres-tf-state PASSCODE=XXX
	cd terraform && terraform state rm cloudfoundry_service_instance.postgres

restore-postgres: terraform-init read-deployment-config ## make dev restore-postgres DB_INSTANCE_GUID="<cf service db-name --guid>" BEFORE_TIME="yyyy-MM-dd hh:mm:ss" TF_VAR_api_docker_image=ghcr.io/dfe-digital/qualified-teachers-api:<COMMIT_SHA> PASSCODE=<auth code from https://login.london.cloud.service.gov.uk/passcode>
	cf target -s ${SPACE} > /dev/null
	$(if $(DB_INSTANCE_GUID), , $(error can only run with DB_INSTANCE_GUID, get it by running `make ${SPACE} get-postgres-instance-guid`))
	$(if $(BEFORE_TIME), , $(error can only run with BEFORE_TIME, eg BEFORE_TIME="2021-09-14 16:00:00"))
	$(eval export TF_VAR_paas_restore_db_from_db_instance=$(DB_INSTANCE_GUID))
	$(eval export TF_VAR_paas_restore_db_from_point_in_time_before=$(BEFORE_TIME))
	echo "Restoring ${POSTGRES_DATABASE_NAME} from $(TF_VAR_paas_restore_db_from_db_instance) before $(TF_VAR_paas_restore_db_from_point_in_time_before)"
	make ${DEPLOY_ENV} terraform-apply

restore-data-from-backup: read-deployment-config # make production restore-data-from-backup CONFIRM_RESTORE=YES BACKUP_FILENAME="qualified-teachers-api-prod-pg-svc-2022-04-28-01"
	@if [[ "$(CONFIRM_RESTORE)" != YES ]]; then echo "Please enter "CONFIRM_RESTORE=YES" to run workflow"; exit 1; fi
	$(eval export AZURE_BACKUP_STORAGE_ACCOUNT_NAME=$(AZURE_BACKUP_STORAGE_ACCOUNT_NAME))
	$(if $(BACKUP_FILENAME), , $(error can only run with BACKUP_FILENAME, eg BACKUP_FILENAME="qualified-teachers-api-prod-pg-svc-2022-04-28-01"))
	bin/download-db-backup ${AZURE_BACKUP_STORAGE_ACCOUNT_NAME} ${AZURE_BACKUP_STORAGE_CONTAINER_NAME} ${BACKUP_FILENAME}.tar.gz
	bin/restore-db ${DEPLOY_ENV} ${CONFIRM_RESTORE} ${SPACE} ${BACKUP_FILENAME}.sql ${POSTGRES_DATABASE_NAME}

terraform-init:
	$(if $(or $(DISABLE_PASSCODE),$(PASSCODE)), , $(error Missing environment variable "PASSCODE", retrieve from https://login.london.cloud.service.gov.uk/passcode))
	$(if $(PLATFORM), , $(error Missing environment variable "PLATFORM"))

	$(eval export TF_VAR_service_name=$(SERVICE_SHORT))
	$(eval export TF_VAR_service_short_name=$(SERVICE_SHORT))
	$(eval export TF_VAR_environment_short_name=$(ENV_SHORT))
	$(eval export TF_VAR_azure_resource_prefix=$(RESOURCE_NAME_PREFIX))

	[[ "${SP_AUTH}" != "true" ]] && az account set -s $(AZURE_SUBSCRIPTION) || true
	terraform -chdir=terraform/$(PLATFORM) init -backend-config workspace_variables/${DEPLOY_ENV}.backend.tfvars -reconfigure

terraform-plan: terraform-init # make [env] terraform-plan init
	terraform -chdir=terraform/$(PLATFORM) plan -var-file workspace_variables/${DEPLOY_ENV}.tfvars.json

terraform-apply: terraform-init
	terraform -chdir=terraform/$(PLATFORM) apply -var-file workspace_variables/${DEPLOY_ENV}.tfvars.json ${AUTO_APPROVE}

terraform-destroy: terraform-init
	terraform -chdir=terraform/$(PLATFORM) destroy -var-file workspace_variables/${DEPLOY_ENV}.tfvars.json ${AUTO_APPROVE}

deploy-azure-resources: set-azure-account # make dev deploy-azure-resources CONFIRM_DEPLOY=1
	$(if $(CONFIRM_DEPLOY), , $(error can only run with CONFIRM_DEPLOY))
	az deployment sub create -l "${REGION}" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${RESOURCE_NAME_PREFIX}${SERVICE_SHORT}tfstate${ENV_SHORT}" "tfStorageContainerName=${SERVICE_SHORT}-tfstate" "dbBackupStorageAccountName=${AZURE_BACKUP_STORAGE_ACCOUNT_NAME}" "dbBackupStorageContainerName=${AZURE_BACKUP_STORAGE_CONTAINER_NAME}" "keyVaultNames=['${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-api-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-authz-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-inf-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-ui-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-worker-kv']"

validate-azure-resources: set-azure-account # make dev validate-azure-resources
	az deployment sub create -l "${REGION}" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/main/azure/resourcedeploy.json" --parameters "resourceGroupName=${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-rg" 'tags=${RG_TAGS}' "tfStorageAccountName=${RESOURCE_NAME_PREFIX}${SERVICE_SHORT}tfstate${ENV_SHORT}" "tfStorageContainerName=${SERVICE_SHORT}-tfstate" "dbBackupStorageAccountName=${AZURE_BACKUP_STORAGE_ACCOUNT_NAME}" "dbBackupStorageContainerName=${AZURE_BACKUP_STORAGE_CONTAINER_NAME}" "keyVaultNames=['${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-api-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-authz-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-inf-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-ui-kv', '${RESOURCE_NAME_PREFIX}-${SERVICE_SHORT}-${ENV_SHORT}-worker-kv']" --what-if


domains-init: bin/terrafile set-azure-pd-subscription ## make [env] domains-init -  terraform init for environment dns/afd resources
	./bin/terrafile -p terraform/domains/environment_domains/vendor/modules -f terraform/domains/environment_domains/config/${DEPLOY_ENV}_Terrafile
	terraform -chdir=terraform/domains/environment_domains init -reconfigure -upgrade -backend-config=config/${DEPLOY_ENV}_backend.tfvars

domains-plan: domains-init ## terraform plan for environment dns/afd resources
	terraform -chdir=terraform/domains/environment_domains plan -var-file config/${DEPLOY_ENV}.tfvars.json

domains-apply: domains-init ## terraform apply for environment dns/afd resources, needs CONFIRM_DEPLOY=1 for production
	terraform -chdir=terraform/domains/environment_domains apply -var-file config/${DEPLOY_ENV}.tfvars.json

domains-infra-init: bin/terrafile set-azure-pd-subscription ## make [domain|aks_domain] domains-infra-init -  terraform init for dns/afd core resources, eg Main FrontDoor resource
	./bin/terrafile -p terraform/domains/infrastructure/vendor/modules -f terraform/domains/infrastructure/config/trs_Terrafile
	terraform -chdir=terraform/domains/infrastructure init -reconfigure -upgrade

domains-infra-plan: domains-infra-init ## terraform plan for dns core resources
	terraform -chdir=terraform/domains/infrastructure plan -var-file config/trs.tfvars.json

domains-infra-apply: domains-infra-init ## terraform apply for dns core resources
	terraform -chdir=terraform/domains/infrastructure apply -var-file config/trs.tfvars.json

domain-azure-resources: set-azure-account # make [domain|aks_domain] domain-azure-resources CONFIRM_DEPLOY=1, creates core DNA/AKS
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
