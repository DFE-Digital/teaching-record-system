# How to restore database from point in time backup

The below steps can be used to restore a database to its point in time backup.
We can restore to any point from 5 minutes to 7 days ago, with a resolution of one second.

## Adding the user to SpaceDeveloper role

The user must be in the SpaceDeveloper role to perform any of the below actions.
A user can be added to the SpaceDeveloper role by anyone in the SpaceManager role using the below command

- CF CLI
`cf set-space-role <firstname.lastname@digital.education.gov.uk|Google SSO UID> dfe <space-name> SpaceDeveloper`

- Makefile

`make <ENV> set-space-developer USER_ID=<user id obtained from $(cf target) command> `

The username of the person being added to the role can be obtained by asking them to run `cf target`, this is usually their @digital.education.gov.uk email address unless they have enabled Google SSO in their GovUK PaaS account.

## Restoring from a point in time backup

### 1. Set the correct space context and stop any apps bound to the database service instance

Set the context for the cf cli commands to the space under which the service exists.

Stop the app that depend on the database service instance using the `cf stop` command. The list of apps in the space can be obtained by running `cf apps`.

This can be done by running Makefile rule as follows:

For example run `make <ENV> stop-app CONFIRM_STOP=1`

### 2. Identify the service instance's GUID

Find the GUID of the database service instance which has been corrupted.

Run Makefile for example `make <ENV> get-postgres-instance-guid`.

After this we need to know the latest timestamp in UTC from which we want to restore using the backup.

### 3. Login to Azure CLI and obtain PaaS passcode

Obtain a passcode from [GOV.UK PaaS](https://login.london.cloud.service.gov.uk/passcode)

Login to Azure using Azure CLI. Run `az login` if not logged in previously. This will take you to the azure portal to authenticate the cli. This is required for reading the terraform state and KeyVault secrets.

### 4. Rename the postgres database

This is because as part of the recovery process we will recreate the service using the same name as before with the backup parameters in one terraform action.

Run `make <ENV> rename-postgres-service CONFIRM_RENAME=1 NEW_NAME_SUFFIX=old`

### 5. Remove existing database service from terrform state

This is needed so that, a new postgres service instance with point in time backup is created instead of the resource in state file.

Run `make <ENV> remove-postgres-tf-state PASSCODE=<CF_SSO_PASSCODE>` where PASSCODE is the PaaS passcode obtained in step 3.

### 6. Deploy new database instance from PaaS backup

This will create a new instance of the postgres database with point in time backup specified by the time passed in.

*Please note in the below command, the timestamp is in UTC, so if we are in BST, make sure to offset the hour by 1, ie: 5:00 PM BST would be 4:00 GMT during summer.*

`make <ENV> restore-postgres PASSCODE=<CF_SSO_PASSCODE> DB_INSTANCE_GUID=<> BEFORE_TIME="<UTC_TIMESTAMP>"`.

*Note: DB_INSTANCE_GUID is the output of step 2. BEFORE_TIME should be in format 2022-04-21 09:00:00*


Running the command above will prompt for few terraform variables if you dont pass it as arguments to the command in **TF_VAR_name_of_variable=VALUE** form.

- var.api_docker_image

    You can pass the image url found from the Github [packages](https://github.com/DFE-Digital/qualified-teachers-api/pkgs/container/qualified-teachers-api). For example **ghcr.io/dfe-digital/qualified-teachers-api:87bef08efd6ea4a0062cff782d9cdf45340ab0ef**

    Find the current version of the Github SHA of image by running `cf app APP_NAME` on CF CLI, where APP_NAME is the name of the app you stopped in step one. Make sure you use the same image as the current one deployed.

- var.api_app_version

    This is just the SHA of the docker image. Make sure you have the same SHA as in var.api_docker_image. For example **87bef08efd6ea4a0062cff782d9cdf45340ab0ef** in the var.api_docker_image variable above.

Review the generated deploy plan and check that the database instance will be created using the specified point in time backup.
Make sure there are no changes to the **api_docker_image** property of the app as we are deploying the app again with the same version as before the incident.

Once the plan is reviewed, type **yes** to initiate the deployment process. This process can take between 10 and 60 mins to complete.  When this is in progress, you can run cf service <db-instance-name> to check the progress. The database service instance will be created and the dependant app will be started on successful completion.

### 7. Delete renamed database instance

Once restore has been ran successfully and checked, make sure to delete the renamed instance created before the restore process was intitated in step 4.

Run `cf delete-service <db-service-instance-name>-old`
