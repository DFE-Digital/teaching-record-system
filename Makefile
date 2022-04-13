dev:
	$(eval DEPLOY_ENV=dev)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-development)

test:
	$(eval DEPLOY_ENV=test)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-test)

pre-production:
	$(eval DEPLOY_ENV=pre-production)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-test)

production:
	$(eval DEPLOY_ENV=production)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-production)

read-keyvault-config:
	$(eval KEY_VAULT_NAME=$(shell jq -r '.key_vault_name' terraform/$(DEPLOY_ENV).tfvars.json))
	$(eval KEY_VAULT_SECRET_NAME=$(shell jq -r '.key_vault_secret_name' terraform/$(DEPLOY_ENV).tfvars.json))

set-azure-account: ${environment}
	echo "Logging on to ${AZURE_SUBSCRIPTION}"
	az account set -s ${AZURE_SUBSCRIPTION}

ci:	## Run in automation environment
	$(eval DISABLE_PASSCODE=true)
	$(eval AUTO_APPROVE=-auto-approve)
	$(eval SP_AUTH=true)

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

terraform-init:
	$(if $(or $(DISABLE_PASSCODE),$(PASSCODE)), , $(error Missing environment variable "PASSCODE", retrieve from https://login.london.cloud.service.gov.uk/passcode))
	[[ "${SP_AUTH}" != "true" ]] && az account set -s $(AZURE_SUBSCRIPTION) || true
	terraform -chdir=terraform init -backend-config ${DEPLOY_ENV}.backend.tfvars -upgrade -reconfigure

terraform-plan: terraform-init
	terraform -chdir=terraform plan -var-file ${DEPLOY_ENV}.tfvars.json

terraform-apply: terraform-init
	terraform -chdir=terraform apply -var-file ${DEPLOY_ENV}.tfvars.json ${AUTO_APPROVE}

terraform-destroy: terraform-init
	terraform -chdir=terraform destroy -var-file ${DEPLOY_ENV}.tfvars.json ${AUTO_APPROVE}
