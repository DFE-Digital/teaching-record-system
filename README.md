# teaching-record-system
The TRS is a database of people who work in Education in the UK. It is the primary source of teaching records for DfE. It holds data to meet UK Government statutory obligations as well as allow individuals working in the UK education system (including, but not limited to) teachers in schools within England, Wales, Scotland and Northern Ireland to access digital services provided by the DfE. It is also a DfE web service used by DfE support teams. It is the source of Teaching Reference Number (TRN) used in a number of Education services and processes both inside and outside of DfE.


## Teaching Reference Number
The TRN is a unique 7-digit reference given to identify a person whose data is in the Teaching Record System (formerly Database Of Qualified Teachers). TRNs are given to trainee teachers, qualified teachers (QTS and many other teaching related qualifications), and anyone eligible for a teaching pension and other related services.

It is a character string of len(7) e.g.:
```
'1234567', '0001234'
```
# High level architecture
* [Logical Data Model](docs/logical-data-model.md)
* [Technical Architecture](docs/trs-technical-architecture-hld.md)
# Data Integrations
* [High Level Data Integrations](docs/trs-data-integrations.md)
* [Low level example using Access Your Teaching Qualifications](docs/c4-diagrams-as-code/trs-core-containers.jpg)
* [Low level example showing Teacher Pensions Service](docs/c4-diagrams-as-code/trs-tps.jpg)

## Authorising Access using GOV.UK One Login
* [Authorisation flow for services using GOVUK.One Login](docs/trs-gov.one-login-flow.md)

## Calling the API

The API is versioned and each endpoint is prefixed with the version number e.g. `/v3/`. V3 is further split into minor versions; a header should be added to your requests indicating which minor version you want e.g.
```
X-Api-Version: 20240101
```

Wherever possible you should call the latest version.

You can view the API specifications for each version by visiting `/swagger` (see [Environments](#environments) below for the base addresses).

An API key is required for calling the API; speak to one of the developers to get one. The key must be passed in an `Authorization` header e.g.
```
Authorization: Bearer your_api_key
```

### Upgrading V3 versions

See the [changelog](CHANGELOG.md) for the details of what has changed between versions.


## Environments

| Name           | API URL                                                     |
|----------------|-------------------------------------------------------------|
| Production     | https://teacher-qualifications-api.education.gov.uk         |
| Pre-production | https://preprod.teacher-qualifications-api.education.gov.uk |
| Development    | https://dev.teacher-qualifications-api.education.gov.uk/    |


## Developer setup

### Software requirements

The API is an ASP.NET Core 8 web application. To develop locally you will need the following installed:
- Visual Studio 2022 (or the .NET 8 SDK and an alternative IDE/editor);
- a local Postgres 15+ instance;
- NPM.

A `justfile` defines various recipes for development. Ensure [just](https://just.systems/) is installed and available on your `$PATH`.

If you're working on infrastructure you will also need:
- make;
- Terraform;
- Azure CLI;
- bash.

### Local tools setup

A `just` recipe will install the required local tools:
```shell
> just install-tools
```

### Blob storage emulator

The Support UI uses Azure Blob Storage for storing files. For local development, you can use the Azure Storage Emulator - Azurite. If you
use Visual Studio 2022 you probably have it installed already otherwise you can install it from [here](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite).

The connection string to Blob Storage will need to be set up to point to the storage emulator:

```shell
> just set-secret StorageConnectionString "UseDevelopmentStorage=true"
```

The containers it needs are listed in the [storage terraform script](teaching-record-system/blob/main/terraform/aks/storage.tf), you will need to create them manually.

You will need to start Azurite before running the Support UI:
```shell
> azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

Git bash:

```shell
$ azurite --silent --location /c/azurite --debug /c/azurite/debug.log
```

### Install Playwright

Playwright is used for end-to-end testing. Install it with a `just` recipe:
```shell
> just install-playwright
```

**Note:** the solution must be built first as the recipe requires the existence of `bin/Debug/net8.0/playwright.ps1` which is generated when the `Microsoft.Playwright` package is built.

### Database setup

Install Postgres then set a connection string configuration entry in user secrets for running the apps and another for running the tests.
Note you should use a different database for tests as the test database will be cleared down whenever tests are run.

e.g.
```shell
# local settings
> just set-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs"

# test settings
> just set-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
```

To set up the initial TRS database schema run:
```shell
> just build
> just cli migrate-db
> just cli add-trn-range --from 1000000 --to 9999999
```

The trs_tests database will be created automatically when running the tests.

#### Database migrations

If you need to change the Postgres database tables, the best way to do this is to update the Model files, in `TeachingRecordSystem.Core\DataStore\Postgres\Models\`, and update the appropriate Entity Framework mapping file in `TeachingRecordSystem.Core\DataStore\Postgres\Mappings\`.

Once the model/mapping files have been changed, a migration file will need to be created - this can be done automatically by running:
```shell
> just ef migrations add <a migration name>
```
where the migration name is conventionally a title-case short description of the change, e.g. `AddUserRoleColumn`.

If you are seeing the error `DbContext has pending changes not covered by a migration` when running the tests or the web application locally, it usually means a change has been made to the model without an appropriate migration file. Running the recipe above to add the migration should fix this.

#### Migrating the local database
In order for the local `trs` database to pick up the change, the migrate recipe will need to be run:

```shell
> just build
> just cli migrate-db
```

**Note:** `just build` needs to be run first to make sure that the latest migration is compiled into the `TeachingRecordSystem.Cli` dll - `just cli migrate-db` will fail if the dll doesn't exist.

The trs_tests database for the tests should be migrated automatically when running the tests.

#### Downgrading the local database
To rollback a series of migrations, use the `--target-migration` parameter to indicate the name of a migration to end on
(the database will be left in the state just after applying this migration)

```shell
> just build
> just cli migrate-db --target-migration BeforeAddingUserRoleColumn
```

#### Regenerating the test cache

The trs_tests database for the tests should be migrated automatically, however sometimes it gets stuck and the tests may fail with the message:
```
Microsoft.EntityFrameworkCore.DbUpdateException : An error occurred while saving the entity changes. See the inner exception for details.
---- Npgsql.PostgresException : <some Postgres error, e.g. missing table or column>
```
If this happens, regenerating the test cache usually fixes this, there's a `just` recipe:
```shell
> just remove-tests-schema-cache
```

#### DQT Reporting database setup

This solution contains a service that synchronises changes from CRM into a SQL Server database used for reporting (this replaces the now-deprecated Data Export Service).
It also synchronises selected tables from TRS.
By default, this is disabled for local development. For the tests to pass, you will need a test database and a connection string defined in user secrets e.g.
```shell
> just set-tests-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReportingTests;Integrated Security=Yes;TrustServerCertificate=True"
```

Your postgres server's `wal_level` must be set to `logical`:
```
trs=# ALTER SYSTEM SET wal_level = logical;
```
You will have to restart the server after amending this configuration.

To run the service locally override the configuration option to run the service and ensure a connection string is provided e.g.
```shell
> just set-secret DqtReporting:RunService true
> just set-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReporting;Integrated Security=Yes;TrustServerCertificate=True"
```
The service will now run as a background service of the `Worker` project.

It is a good idea to remove the replication slot when you're not working on this service to avoid a backlog on unprocessed changes accumulating in postgres.
```shell
> just set-secret DqtReporting:RunService false
> just cli drop-dqt-reporting-replication-slot
```


### Admin user setup

Add yourself to your local database as an administrator:
```shell
> just create-admin "your.name@education.gov.uk" "Your Name"
```


### External dependencies

There are several external dependencies required for local development; these are listed below.
Ask a developer on the team for the user secrets for these dependencies.

#### Dynamics CRM

The `build` CRM environment is used for local development and automated tests.

The secrets you will need to set are as follows:
```shell
# local settings
> just set-secret ConnectionStrings:Crm "AuthType=ClientSecret;Url=https://ent-dqt-build.crm4.dynamics.com;ClientId=<REDACTED>;ClientSecret=<REDACTED>;RequireNewInstance=true"
> just set-secret TrsSyncService:CrmConnectionString "AuthType=ClientSecret;Url=https://ent-dqt-build.crm4.dynamics.com;ClientId=<REDACTED>;ClientSecret=<REDACTED>;RequireNewInstance=true"
> just set-secret CrmClientId "<REDACTED>"
> just set-secret CrmClientSecret "<REDACTED>"
> just set-secret CrmUrl "https://ent-dqt-build.crm4.dynamics.com"

# test settings
> just set-tests-secret ConnectionStrings:Crm "AuthType=ClientSecret;Url=https://ent-dqt-build.crm4.dynamics.com;ClientId=<REDACTED>;ClientSecret=<REDACTED>;RequireNewInstance=true"
> just set-tests-secret TrsSyncService:CrmConnectionString "AuthType=ClientSecret;Url=https://ent-dqt-build.crm4.dynamics.com;ClientId=<REDACTED>;ClientSecret=<REDACTED>;RequireNewInstance=true"
> just set-tests-secret CrmClientId "<REDACTED>"
> just set-tests-secret CrmClientSecret "<REDACTED>"
> just set-tests-secret CrmUrl "https://ent-dqt-build.crm4.dynamics.com"
```

#### TRN Generation API

The API calls the TRN Generation API to generate a TRNs.

The secrets you will need to set are as follows:
```shell
# local settings
> just set-secret TrnGenerationApi:BaseAddress "https://dev.trn-generation-api.education.gov.uk/"
just set-secret TrnGenerationApi:ApiKey "<REDACTED>"

# test settings
> just set-tests-secret TrnGenerationApi:BaseAddress "https://dev.trn-generation-api.education.gov.uk/"
> just set-tests-secret TrnGenerationApi:ApiKey "<REDACTED>"
```

#### Azure AD

Azure AD is used for authenticating users in the Support UI.

The secrets you will need to set are as follows:

```shell
> just set-secret AzureAd:ClientSecret "<REDACTED>"
> just set-secret AzureAd:ClientId "<REDACTED>"
```

#### Other settings

There are additional secrets you will need to set are as follows:

```shell
# local settings
> just set-secret AccessYourTeachingQualifications:BaseAddress "https://dev.access-your-teaching-qualifications.education.gov.uk/"
> just set-secret AccessYourTeachingQualifications:StartUrlPath "/qualifications/start"

> just set-secret GetAnIdentity:WebHookClientSecret "dummy"
> just set-secret GetAnIdentity:TokenEndpoint "https://dev.teaching-identity.education.gov.uk/connect/token"
> just set-secret GetAnIdentity:ClientSecret "<REDACTED>"
> just set-secret GetAnIdentity:ClientId "dqt-api"
> just set-secret GetAnIdentity:BaseAddress "https://dev.teaching-identity.education.gov.uk/"

> just set-secret Webhooks:CanonicalDomain "https://localhost:5001"
> just set-secret Webhooks:SigningKeyId "devkey"
> just set-secret Webhooks:Keys:0:KeyId "devkey"
> just set-secret Webhooks:Keys:0:CertificatePem "<REDACTED>"
> just set-secret Webhooks:Keys:0:PrivateKeyPem "<REDACTED>"

> just set-secret BuildEnvLockBlobUri "https://s165d01inttests.blob.core.windows.net/leases/build.lock"
> just set-secret BuildEnvLockBlobSasToken "<REDACTED>"

# test settings
> just set-tests-secret BuildEnvLockBlobUri "https://s165d01inttests.blob.core.windows.net/leases/build.lock"
> just set-tests-secret BuildEnvLockBlobSasToken "<REDACTED>"
```

## CRM code generation

A tool is used to generated proxy classes for the entities defined within the DQT CRM.
The tool generates the `TeachingRecordSystem.Dqt.Models.GeneratedCode.cs` and `TeachingRecordSystem.Dqt.Models.GeneratedOptionSets.cs` files.
A configuration file at `crm_attributes.json` whitelists the entities and their attributes which are included on the generated types.

Run `just generate-crm-models` to run the code generator against the `build` environment using the configuration file above.
The CRM user secrets described within [Developer setup](#dynamics-crm) must be correctly set for the tool to run successfully.
The tool is a .NET Framework application and requires .NET 4.6.

**Note:** Make sure Visual Studio is closed before running this recipe as otherwise it may fail due to these files being already in use by the Visual Studio process.

## Environment configuration

Environment-specific configuration is stored in Key Vault inside a single JSON-encoded key named 'AppConfig'.
This is retrieved via Terraform at deployment time and is set as an environment variable so it can be accessed by the application.

## Local development

When developing locally there are a number of recipes that may be useful (`just` with no parameters lists all available recipes):

```shell
> just
watch-api                  # Run the API project in Development mode and watch for file changes
watch-authz                # Run the AuthorizeAccess project in Development mode and watch for file changes
watch-ui                   # Run the UI project in Development mode and watch for file changes
watch-ui-sass              # Watch for file changes and compile any SASS files that have changed
watch-worker               # Run the Worker project in Development mode and watch for file changes
```

When working on the Support UI project it may be useful to have both `watch-ui` and `watch-ui-sass` running in parallel (e.g. in separate terminal windows) - this should pick up on any changes and hot reload the web application, refreshing the web browser.

The Visual Studio debugger can also be attached to the watched web application via `Debug > Attach to Process...` and selecting the `TeachingRecordSystem.SupportUi.exe` process.

Sometimes the `watch-ui` process may fail with the error message `ArgumentOutOfRangeException: Token 2007ffe is not valid in the scope of module System.ModuleHandle` - it is unclear what causes this but it is usually resolved by killing the watch process, cleaning the solution and restarting the watch process.

## Formatting

Pull request builds will run format checks on .NET and Terraform code changes; if there are any issues the build will fail.

Before committing you can format any changed files by running:
```shell
> just format-changed
```

To format the entire codebase run
```shell
> just format
```

For either of these recipes to work, the [Terraform tool must be installed](https://developer.hashicorp.com/terraform/install?product_intent=terraform) - on Windows, download the binary, copy into an appropriate location and update the PATH environment variable to point to the downloaded file.

**Note:** `just format` and `just format-changed` call `dotnet dotnet-format` - this is different to `dotnet format` so be aware that calling `dotnet format` may produce different results.

### Visual Studio Code Cleanup
If you're using Visual Studio 2022 you can also set up Code Cleanup, this will use the settings defined in the `.editorconfig` file in the repository root (this is also added to the Solution Items folder in the solution).

To set this up, go to `Tools > Options > Text Editor > Code Cleanup` and click `Configure Code Cleanup`. This will present you with a window with two profiles to configure, the easiest thing to do is to select `Profile 1 (default)`, and select the following from "Available fixers" (bottom panel), and add them to the "Included fixers" (top panel):

- Format document

Now you can use the "paintbrush" icon in the text editor status bar (to the left of the horizontal scrollbar) to format the document you're working on (keyboard shortcut: Ctrl+K, Ctrl+E) - or you can go to `Tools > Options > Text Editor > Code Cleanup` and check `Run Code Cleanup profile on Save`.

# Using Kubernetes to connect to dev/pre-prod environments

If you need to connect to one of the environments, for example to create yourself an admin account, you'll need to get Azure CLI and Kubernetes set up locally.

1. Install [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest&pivots=msi)
1. Log in to Azure locally (using Windows command prompt - doesn't work in Git bash):

   ```shell
   > az login --tenant 9c7d9dd3-840c-4b3f-818e-552865082e16
   Select the account you want to log in with. For more information on login with Azure CLI, see https://go.microsoft.com/fwlink/?linkid=2271136
   ```
1. Choose your account and authenticate
1. Select the default subscription to the subscription you want to access, e.g. `s189-teacher-services-cloud-test` (`20da9d12-7ee1-42bb-b969-3fe9112964a7`)

   ```shell
   Retrieving tenants and subscriptions for the selection...
   The following tenants don't contain accessible subscriptions. Use `az login --allow-no-subscriptions` to have tenant level access.
   fad277c9-c60a-4da1-b5f3-b3b8b34a82f9 'Department for Education'
   
   [Tenant and subscription selection]
   
   No     Subscription name                     Subscription ID                       Tenant
   -----  ------------------------------------  ------------------------------------  ------------------------------------
   [1]    N/A(tenant level account)             fad277c9-c60a-4da1-b5f3-b3b8b34a82f9  fad277c9-c60a-4da1-b5f3-b3b8b34a82f9

   [...]

   [15]   s189-teacher-services-cloud-devel...  5c83eb53-a94f-4778-b258-1f33efe49655  DfE Platform Identity
   [16]   s189-teacher-services-cloud-produ...  3c033a0c-7a1c-4653-93cb-0f2a9f57a391  DfE Platform Identity
   [17]   s189-teacher-services-cloud-test      20da9d12-7ee1-42bb-b969-3fe9112964a7  DfE Platform Identity   <-- this one
   
   The default is marked with an *; the default tenant is 'DfE Platform Identity' and subscription is [...].
   
   Select a subscription and tenant (Type a number or Enter for no changes): 17
   ```

   Alternatively, do

   ```shell
   > az account set --subscription s189-teacher-services-cloud-test
   ```

1. Pull down the credentials to authenticate with Kubernetes, in this case we want `s189t01-tsc-test-aks` in resource group `s189t01-tsc-ts-rg` (YMMV)

   ```shell
   > az aks get-credentials --overwrite-existing -g s189t01-tsc-ts-rg --name s189t01-tsc-test-aks
   Merged "s189t01-tsc-test-aks" as current context in <your user directory>\.kube\config
   The kubeconfig uses devicecode authentication which requires kubelogin. Please install kubelogin from https://github.com/Azure/kubelogin or run 'az aks install-cli' to install both kubectl and kubelogin. If devicecode login fails, try running 'kubelogin convert-kubeconfig -l azurecli' to unblock yourself.
   ```

1. As the output suggests, install kubectl/kubeconfig via `az aks install`  - this didn't work for me due to certificate issues, so manually:
   1. [install kubectl](https://cjyabraham.gitlab.io/docs/tasks/tools/install-kubectl/#install-kubectl) - on Windows, download the binary directly from [this link](https://storage.googleapis.com/kubernetes-release/release/v1.11.0/bin/windows/amd64/kubectl.exe) and add it to your PATH
   1. [install kubelogin](https://github.com/Azure/kubelogin) - on Windows:
      ```shell
      > winget install --id=Kubernetes.kubectl  -e
      > winget install --id=Microsoft.Azure.Kubelogin  -e
      ```

   1. Restart the command window to pick up the additions to PATH

1. Convert kubeconfig to Exec plugin ([more info](https://github.com/Azure/kubelogin/blob/main/docs/book/src/cli/convert-kubeconfig.md)):
1. ```shell
   > kubelogin convert-kubeconfig -l azurecli
   ```
1. Now you should be able to connect to the Kubernetes cluster. To see the pods available to connect to call `get pods` (if you see a certificate error you might need to add the `--insecure-skip-tls-verify` argument):

   ```shell
   > kubectl get pods -n tra-development --insecure-skip-tls-verify
   NAME                                              READY     STATUS      RESTARTS       AGE
   [...]                                             
   trs-dev-api-7bd486dcdc-z5zvw                      1/1       Running     0              2m18s
   trs-dev-authz-6cd4545d94-fwfg7                    1/1       Running     0              2m18s
   trs-dev-migrations-gvvpb                          0/1       Completed   0              2m46s
   trs-dev-ui-6969874cc9-j96mz                       1/1       Running     0              2m18s
   trs-dev-worker-7444b9cd96-v52bk                   1/1       Running     0              2m17s
   ```
1. Indentify the pod you want to connect to, in this case we want the Dev UI to create an admin account: `trs-dev-ui-6969874cc9-j96mz`
1. Execute a bash shell on the pod (again, add `--insecure-skip-tls-verify` if there are cert issues):

   ```shell
   > kubectl exec -it trs-dev-ui-6969874cc9-j96mz -n tra-development --insecure-skip-tls-verify -- /bin/ash
   ```

1. Finally, use the TRS CLI to create yourself an admin account:

   ```shell
   /Apps $ trscli create-admin --email your.email@education.gov.uk --name "Your Name"
   ```
