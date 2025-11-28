# Disaster recovery testing
This is an accompaniment to the [Teacher Services Disaster Recovery document](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery.md) and [Teacher Services Disaster Recovery testing document](https://github.com/DFE-Digital/teacher-services-cloud/blob/main/documentation/disaster-recovery-testing.md)

See also the [Data recovery testing document](https://educationgovuk.sharepoint.com.mcas.ms/:w:/r/sites/TRATransformationTeamDocs/_layouts/15/Doc.aspx?sourcedoc=%7BEEA3DDFA-B4E2-4987-9815-D9D7C644636E%7D&file=Draft_TRS_DR_Checklist.docx&action=default&mobileredirect=true)

## Scenario 1: [Loss of database server](https://technical-guidance.education.gov.uk/infrastructure/disaster-recovery/#loss-of-database-instance)
Prerequisites: 
* Create a backup of postgres server `s189t01-trs-pt-pg` on environment `pentest` (remember backup filename) - `pentest` environment does not have scheduled backups unlike production
* Delete postgres server `s189t01-trs-pt-pg` on environment `pentest`

### Start the incident process (if not already in progress)
Skip this for DR testing
### Freeze pipeline
Skip this for DR testing
### Enable maintenance mode
Skip this for DR testing
### Recreate the lost postgres database server
#### Option 1. Recover from Azure backups
Enviroment: `pentest`
Restore to production: `false`
Deleted server: `s189t01-trs-pt-pg`
Note: the document is confusing, a point-in-time restore can only be run 10 minutes after the database has been deleted (something to do with Azure) - but the point in time should be for a time *before* the database was deleted
Note: the GitHub action will complete but this will only trigger a request to Azure to restore the server, which will take an unspecified amount of time.
#### Option 2. Recreate via terraform and restore from scheduled offline backup
Failed: no access to s189-teacher-services-cloud-test subscription on Monitor | Diagnostic settings blade
No maintenance page so ignore steps 1-3 in "As the maintenance page has been enabled, you will need to:" - BUT deploy workflow will recreate DB server so do this on `main` branch (`docker_image` parameter can be ignored)
Deploy workflow will likely fail due to diagnostic settings existing for `s189t01-trs-pt-pg`, however the first run should create the `s189t01-trs-pt-pg` server, and diagnotic settings can then be accessed via the "Azure Database for PostgreSQL flexible server" blade for `s189t01-trs-pt-pg` in Monitoring > Diagnostic Settings and can be deleted there. Then run the deploy workflow again

If the following error occurs:
```
Error: A resource with the ID "/subscriptions/***/resourceGroups/s189t01-trs-pt-rg/providers/Microsoft.DBforPostgreSQL/flexibleServers/s189t01-trs-pt-pg/databases/trs_pentest" already exists - to be managed via Terraform this resource needs to be imported into the State. Please see the resource documentation for "azurerm_postgresql_flexible_server_database" for more information.
```

You will have to go into Azure portal and delete the `trs_pentest` database (the server should have been restored with the first run of the deploy workflow)
### Restore the data from previous backup in Azure storage
Use backup file created in prerequisites step
### Validate app
See [Data recovery testing document](https://educationgovuk.sharepoint.com.mcas.ms/:w:/r/sites/TRATransformationTeamDocs/_layouts/15/Doc.aspx?sourcedoc=%7BEEA3DDFA-B4E2-4987-9815-D9D7C644636E%7D&file=Draft_TRS_DR_Checklist.docx&action=default&mobileredirect=true) for steps
### Disable maintenance mode
Skip this for DR testing
### Unfreeze pipeline
Skip this for DR testing
## Scenario 2: [Loss of data](https://technical-guidance.education.gov.uk/infrastructure/disaster-recovery/#data-corruption)
### Stop the service as soon as possible
### Start the incident process (if not already in progress)
### Freeze pipeline
### Enable maintenance mode
### Consider backing up the database
### Restore postgres database
### Upload restored database to Azure storage
### Validate data
### Restore data into the live server
### Restart applications
### Validate app
### Disable maintenance mode
### Unfreeze pipeline
### Tidy up
