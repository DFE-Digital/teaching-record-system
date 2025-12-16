# TRS Disaster Recovery testing
This is an accompaniment to the [Teacher Services Disaster Recovery testing documentation](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery-testing.md)
and [Teacher Services Disaster Recovery documentation](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md).

See also the [Data recovery testing document](https://educationgovuk.sharepoint.com.mcas.ms/:w:/r/sites/TRATransformationTeamDocs/_layouts/15/Doc.aspx?sourcedoc=%7BEEA3DDFA-B4E2-4987-9815-D9D7C644636E%7D&file=Draft_TRS_DR_Checklist.docx&action=default&mobileredirect=true).
## Prerequisites
We use the `pentest` environment for doing disaster recovery testing. Unlike production, the `pentest` environment does not have scheduled backups so we will need to manually create a backup prior to testing.

* Create a backup of the postgres server using the [Backup database to Azure storage action](https://github.com/DFE-Digital/teaching-record-system/actions/workflows/backup-db.yml):
  * Environment: `pentest`
  * Backup file name: (leave blank)
  * Database server name: `s189t01-trs-pt-pg`
* Once complete, view the *Backup database summary* and copy the backup filename

## [Scenario 1: Loss of database server](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery-testing.md#scenario-1-loss-of-database-instance)
### [Delete the postgres database instance](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery-testing.md#delete-the-postgres-database-instance)
* Log onto Azure Portal and delete postgres server `s189t01-trs-pt-pg` on environment `pentest`

### [Start the incident process (if not already in progress)](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#start-the-incident-process-if-not-already-in-progress)
* Skip this step for DR testing
### [Freeze pipeline](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#freeze-pipeline)
* Skip this step for DR testing - there are no active pipelines merging into the `pentest` environment
### [Enable maintenance mode](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#enable-maintenance-mode)
* Skip this step - TRS does not have a maintenance mode
### [Recreate the lost postgres database server](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#recreate-the-lost-postgres-database-server)
#### Option 1. Recover from Azure backups
* Run the [Recover deleted postgres database workflow](https://github.com/DFE-Digital/teaching-record-system/actions/workflows/restore-deleted-postgres.yml):
  * Enviroment to restore: `pentest`
  * Restore to production: `false`
  * Restore point in time: This should be a point in time after the backup in Prerequisites section was created but before the database was deleted
  * Deleted postgres server: `s189t01-trs-pt-pg`

  **Note:** a point-in-time restore can only be run 10 minutes after the database has been deleted (something to do with Azure) - but the point in time should be for a time *before* the database was deleted
 
  **Note also:** the GitHub action will complete but this will only trigger a request to Azure to restore the server, which will take an unspecified amount of time.

#### Option 2. Recreate via terraform and restore from scheduled offline backup
1. Check there aren't any diagnostic settings on [Azure Portal](https://portal.azure.com/#view/Microsoft_Azure_Monitoring/AzureMonitoringBrowseBlade/~/diagnosticsLogs)
   * Subscription: `s189-teacher-services-cloud-test`
   * Resource group: `s189t01-trs-pt-rg`
   * Database: `s189t01-trs-pt-pg-ptr`
2. Ignore the terraform changes as these are just to allow deployment to continue while in maintenenace mode - TRS does not have a maintenance mode
3. Run the [Build and deploy workflow](https://github.com/DFE-Digital/teaching-record-system/actions/workflows/main.yml) against `main` branch
3. If the following error occurs:
   ```
   Error: A resource with the ID "/subscriptions/***/resourceGroups/s189t01-trs-pt-rg/providers/Microsoft.DBforPostgreSQL/flexibleServers/s189t01-trs-pt-pg/databases/trs_pentest" already exists - to be managed via Terraform this resource needs to be imported into the State. Please see the resource documentation for "azurerm_postgresql_flexible_server_database" for more information.
   ```

   You will have to go into Azure portal and delete the `trs_pentest` database and retry step 3
### [Restore the data from previous backup in Azure storage](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#restore-the-data-from-previous-backup-in-azure-storage)
* Ignore this if you used Option 1 in the previous section.
* Run the [Restore database from Azure storage workflow](https://github.com/DFE-Digital/teaching-record-system/actions/workflows/postgres-restore.yml):
  * Enviroment to restore: `pentest`
  * Restore to production: `false`
  * Name of the backup file: Backup file created in Prerequisites section
### Validate app
* See [Data recovery testing document](https://educationgovuk.sharepoint.com.mcas.ms/:w:/r/sites/TRATransformationTeamDocs/_layouts/15/Doc.aspx?sourcedoc=%7BEEA3DDFA-B4E2-4987-9815-D9D7C644636E%7D&file=Draft_TRS_DR_Checklist.docx&action=default&mobileredirect=true) for steps
### Disable maintenance mode
* Skip this step - TRS does not have a maintenance mode
### Unfreeze pipeline
* Skip this step for DR testing - there are no active pipelines merging into the `pentest` environment
## [Scenario 2: Loss of data](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#scenario-2-loss-of-data)
### [Stop the service as soon as possible](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#stop-the-service-as-soon-as-possible)
* Connect to the `pentest` environment as in [Connecting to environments](connecting-to-environments.md#connecting-to-pentest-environment-for-disaster-recovery):
   * Subscription: `s189-teacher-services-cloud-test`
   * Resource group: `s189t01-tsc-pt-rg`
   * Cluster: `s189t01-tsc-platform-test-aks`
   * Namespace: `development`

   e.g.:
   ```
   az account set --subscription s189-teacher-services-cloud-test
   az aks get-credentials --overwrite-existing -g s189t01-tsc-pt-rg --name s189t01-tsc-platform-test-aks
   kubectl get pods -n development --insecure-skip-tls-verify

   kubectl -n development get deployments
   kubectl -n development scale deployment trs-pentest-ui-xxxxxxxxxx-xxxxx --replicas 0
   kubectl -n development scale deployment trs-pentest-worker-xxxxxxxxxx-xxxxx --replicas 0
   ```
### [Start the incident process (if not already in progress)](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#start-the-incident-process-if-not-already-in-progress-1)
* Skip this step for DR testing
### [Freeze pipeline](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#freeze-pipeline-1)
* Skip this step for DR testing - there are no active pipelines merging into the `pentest` environment
### [Enable maintenance mode](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#enable-maintenance-mode-1)
* Skip this step - TRS does not have a maintenance mode
### [Consider backing up the database](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#consider-backing-up-the-database)
* Skip this step for DR testing
### [Restore postgres database](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#restore-postgres-database)
* Run the [Restore database from point in time to new database server workflow](https://github.com/DFE-Digital/teaching-record-system/actions/workflows/postgres-ptr.yml)
  * Enviroment to restore: `pentest`
  * Restore to production: `false`
  * Restore point in time: This should be a point in time after the backup in Prerequisites section was created 
  * Name of the new postgres server: (any name as long as it's different to existing server names)
### [Upload restored database to Azure storage](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#upload-restored-database-to-azure-storage)
* Run the [Backup database to Azure storage workflow](https://github.com/DFE-Digital/teaching-record-system/actions/workflows/backup-db.yml)
  * Enviroment to backup: `pentest`
  * Backup file name: (leave blank)
  * Database server name: Name of the server created in previous step
* Once complete, view the *Backup database summary* and copy the backup filename
### [Validate data](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#validate-data)
* Skip this step for DR testing - `make` does not work on Windows machines
### [Restore data into the live server](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#restore-data-into-the-live-server)
### [Restart applications](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#restart-applications)
* Connect to the `pentest` environment as in [Connecting to environments](connecting-to-environments.md#connecting-to-pentest-environment-for-disaster-recovery):
   * Subscription: `s189-teacher-services-cloud-test`
   * Resource group: `s189t01-tsc-pt-rg`
   * Cluster: `s189t01-tsc-platform-test-aks`
   * Namespace: `development`

   e.g.:
   ```
   az account set --subscription s189-teacher-services-cloud-test
   az aks get-credentials --overwrite-existing -g s189t01-tsc-pt-rg --name s189t01-tsc-platform-test-aks
   kubectl get pods -n development --insecure-skip-tls-verify

   kubectl -n development get deployments
   kubectl -n development scale deployment trs-pentest-ui-xxxxxxxxxx-xxxxx --replicas 1
   kubectl -n development scale deployment trs-pentest-worker-xxxxxxxxxx-xxxxx --replicas 1
   ```
### [Validate app](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#validate-app-1)
* See [Data recovery testing document](https://educationgovuk.sharepoint.com.mcas.ms/:w:/r/sites/TRATransformationTeamDocs/_layouts/15/Doc.aspx?sourcedoc=%7BEEA3DDFA-B4E2-4987-9815-D9D7C644636E%7D&file=Draft_TRS_DR_Checklist.docx&action=default&mobileredirect=true) for steps
### [Disable maintenance mode](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#disable-maintenance-mode-1)
* Skip this step - TRS does not have a maintenance mode
### [Unfreeze pipeline](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#unfreeze-pipeline-1)
* Skip this step for DR testing - there are no active pipelines merging into the `pentest` environment
### [Tidy up](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md#tidy-up)
* In Azure Portal, delete the server created in "Restore postgres database" step above
