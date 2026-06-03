# teaching-record-system
The TRS is a database of people who work in Education in the UK.
It is the primary source of teaching records for DfE.
It holds data to meet UK Government statutory obligations as well as allow individuals working in the UK education system (including, but not limited to)
teachers in schools within England, Wales, Scotland and Northern Ireland to access digital services provided by the DfE.
It is also a DfE web service used by DfE support teams.
It is the source of Teacher Reference Number (TRN) used in a number of Education services and processes both inside and outside of DfE.


## Teacher Reference Number
The TRN is a unique 7-digit reference given to identify a person whose data is in the Teaching Record System (formerly Database Of Qualified Teachers).
TRNs are given to trainee teachers, qualified teachers (QTS and many other teaching related qualifications), and anyone eligible for a teaching pension
and other related services.

It is a 7 character string e.g.:
```
1234567
0001234
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

The API is versioned and each endpoint is prefixed with the version number e.g. `/v3/`.
V3 is further split into minor versions; a header should be added to your requests indicating which minor version you want e.g.
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
- Visual Studio 2022 (or the .NET 10 SDK and an alternative IDE/editor);
- a local Postgres 17+ instance.

A `justfile` defines various recipes for development. Ensure [just](https://just.systems/) is installed and available on your `$PATH`.

For interacting or working on the infrastructure you will also need:
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

Install Postgres then set a connection string configuration entry in user secrets for running the apps and (optionally) another for running the tests.
Note you should use a different database for tests as the test database will be cleared down whenever tests are run.
If you don't set a connection string for the tests then a test container will be used instead (this requires Docker to be installed and running).

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
> just ef database update
> just cli add-trn-range --from 1000000 --to 9999999
```

The trs_tests database will be created automatically when running the tests.

#### Database migrations

If you need to change the Postgres database tables, update the Model files in `TeachingRecordSystem.Core\DataStore\Postgres\Models\`
and update the appropriate Entity Framework mapping file in `TeachingRecordSystem.Core\DataStore\Postgres\Mappings\`.

Once the model/mapping files have been changed, a migration file will need to be created. This can be done by running:
```shell
> just ef migrations add <a migration name>
```
where the migration name is conventionally a title-case short description of the change, e.g. `AddUserRoleColumn`.

If you are seeing the error `DbContext has pending changes not covered by a migration` when running the tests or the web application locally,
it usually means a change has been made to the model without an appropriate migration file. Running the recipe above to add the migration should fix this.

#### Migrating the local database
In order for the local `trs` database to pick up the change, the migrate recipe will need to be run:

```shell
> just ef database update
```

The trs_tests database for the tests should be migrated automatically when running the tests.

#### Downgrading the local database
To rollback a series of migrations, add an additional argument to the `ef database update` command to indicate the name of a migration to end on
(the database will be left in the state just after applying this migration).

```shell
> just ef database update <BeforeAddingUserRoleColumn>
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


### Admin user setup

Add yourself to your local database as an administrator:
```shell
> just create-admin "your.name@education.gov.uk" "Your Name"
```


### External dependencies

There are several external dependencies required for local development; these are listed below.
Ask a developer on the team for the user secrets for these dependencies.

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

## Environment configuration

Non-sensitive configuration is stored in `appsettings.json` files.
Configuration that applies to all the apps is defined in `appsettings.aks_ENV_shared.json`.
Application-specific configuration is defined in `appsettings.aks_ENV.json` in the respective project directory.

Sensitive configuration is stored in Key Vault.
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

### Visual Studio Code Cleanup
If you're using Visual Studio 2022 you can also set up Code Cleanup, this will use the settings defined in the `.editorconfig` file in the repository root (this is also added to the Solution Items folder in the solution).

To set this up, go to `Tools > Options > Text Editor > Code Cleanup` and click `Configure Code Cleanup`. This will present you with a window with two profiles to configure, the easiest thing to do is to select `Profile 1 (default)`, and select the following from "Available fixers" (bottom panel), and add them to the "Included fixers" (top panel):

- Format document

Now you can use the "paintbrush" icon in the text editor status bar (to the left of the horizontal scrollbar) to format the document you're working on (keyboard shortcut: Ctrl+K, Ctrl+E) - or you can go to `Tools > Options > Text Editor > Code Cleanup` and check `Run Code Cleanup profile on Save`.
