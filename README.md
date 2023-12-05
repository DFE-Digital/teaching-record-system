# teaching-record-system

Provides an API over the Database of Qualified Teachers (DQT).


## Calling the API

The API is versioned and each endpoint is prefixed with the version number e.g. `/v2/`. You can view the API specifications for each version by visiting `/swagger` (see [Environments](#environments) below for the base addresses).

An API key is required for calling the API; speak to one of the developers to get one. The key must be passed in an `Authorization` header e.g.
```
Authorization: Bearer your_api_key
```


## Environments

| Name           | URL                                                         |
| -------------- | ----------------------------------------------------------- |
| Production     | https://teacher-qualifications-api.education.gov.uk         |
| Pre-production | https://preprod.teacher-qualifications-api.education.gov.uk |
| Test           | https://test.teacher-qualifications-api.education.gov.uk/   |
| Development    | https://dev.teacher-qualifications-api.education.gov.uk/    |


## Developer setup

### Software requirements

The API is an ASP.NET Core 8 web application. To develop locally you will need the following installed:
- Visual Studio 2022 (or the .NET 8 SDK and an alternative IDE/editor);
- a local Postgres 13+ instance.
- [SASS]( https://sass-lang.com/install).

A `justfile` defines various recipes for development. Ensure [just](https://just.systems/) is installed and available on your `$PATH` as well as [PowerShell](https://microsoft.com/PowerShell).

If you're working on infrastructure you will also need:
- make;
- Terraform;
- bash.

### Local tools setup

dotnet-format is required for linting. A `just` recipe will install it:
```shell
just install-tools
```

### Database setup

Install Postgres then set a connection string configuration entry in user secrets.
In addition, set a connection string for a different database for the test projects.

e.g.
```shell
just set-api-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs"
just set-ui-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs"

just set-core-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
just set-api-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
just set-dqt-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
just set-ui-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
just set-ui-e2e-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
```

To set up the initial database schema run:
```shell
just build
just cli migrate-db
```

The databases will be created automatically when running the tests.

#### DQT Reporting database setup

This solution contains a service that synchronises changes from CRM into a SQL Server database used for reporting (this replaces the now-deprecated Data Export Service). By default this is disabled for local development. For the tests to pass, you will need a test database and a connection string defined in user secrets e.g.
```shell
just set-dqt-tests-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReportingTests;Integrated Security=Yes;TrustServerCertificate=True"
```

To run the service locally, override the configuration option to run the service and ensure a connection string is provided for the `Api` project e.g.
```shell
just set-api-secret DqtReporting:RunService true
just set-api-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReporting;Integrated Security=Yes;TrustServerCertificate=True"
```
The service will run as a background service of the `Api` project.


### Admin user setup

Add yourself to your local database as an administrator:
```shell
just create-admin "your.name@education.gov.uk" "Your Name"
```


### External dependencies

#### Dynamics CRM

The `build` CRM environment is used for local development and automated tests. Connection information should be stored in user secrets in the `ConnectionStrings:Crm` key. Secrets must be added for both the `Api` project and the `Dqt.Tests` project.
In addition, the `SupportUi` project needs the `CrmUrl` configuration entry defining; the Azure AD secrets are used for authentication.

Secrets can be set using `just` recipes e.g.
```shell
just set-api-secret ConnectionStrings:Crm "the_connection_string"
just set-dqt-tests-secret ConnectionStrings:Crm "the_connection_string"
just set-dqt-tests-secret BuildEnvLockBlobUri "lock_blob_uri"
just set-dqt-tests-secret BuildEnvLockBlobSasToken "lock_blob_sas_token"
just set-ui-secret CrmUrl "the_environment_url"
```
Ask a developer on the team for the user secrets for this environment.

#### TRN Generation API

The DQT API calls a TRN Generation API to generate a TRNs. Configuration should be stored in user secrets in the `TrnGenerationApi:BaseAddress` and `TrnGeneration:ApiKey` keys. Secrets must be added for both the `Api` project and the `Dqt.Tests` project. Secrets can be set using `just` recipes e.g.
```shell
just set-api-secret TrnGenerationApi:BaseAddress "https://the-trn-generation-url"
just set-api-secret TrnGenerationApi:ApiKey "the-api-key"
just set-dqt-tests-secret TrnGenerationApi:BaseAddress "https://the-trn-generation-url"
just set-dqt-tests-secret TrnGenerationApi:ApiKey "the-api-key"
```
Ask a developer on the team for the user secrets for this environment.

#### Azure AD

Azure AD is used for authentication for the Support UI. The client ID and secret for local development should be stored in user secrets.
```shell
just set-ui-secret AzureAd:ClientId "the_client_id"
just set-ui-secret AzureAd:ClientSecret "the_client_secret"
```
Ask a developer on the team for the user secrets for this environment.


## CRM code generation

A tool is used to generated proxy classes for the entities defined within the DQT CRM.
The tool generates the `TeachingRecordSystem.Dqt.Models.GeneratedCode.cs` and `TeachingRecordSystem.Dqt.Models.GeneratedOptionSets.cs` files.
A configuration file at `crm_attributes.json` whitelists the entities and their attributes which are included on the generated types.

Run `just generate-crm-models` to run the code generator against the `build` environment using the configuration file above.
The CRM user secrets described within [Developer setup](#dynamics-crm) must be correctly set for the tool to run successfully.
The tool is a .NET Framework application and requires .NET 4.6.


## Environment configuration

Environment-specific configuration is stored in Key Vault inside a single JSON-encoded key named 'APP-CONFIG'.
This is retrieved via Terraform at deployment time and is set as an environment variable so it can be accessed by the application.

A helper script is provided to simplify adding or amending configuration for a specific environment - `Set-AppConfigSecret.ps1`.
For this to run successfully you will need the Azure CLI. You need to be authenticated to access the s165 subscriptions before running the script.

By default the script runs read-only; it calculates the new JSON configuration and print out the result to the console. To commit the change add the `-Commit` switch.

### Example - adding an API client

```shell
./scripts/Set-AppConfigSecret -EnvironmentName dev -AzureSubscription s165-teachingqualificationsservice-development -ConfigKey ApiClients:new_client:ApiKey -ConfigValue super-secret-key
```

```diff
 {
   "ApiClients": {
     "client1": {
       "ApiKey": "key1"
     },
     "client2": {
       "ApiKey": "key2"
-    }
+    },
+    "new_client": {
+      "ApiKey": "super_secret_key"
+    }
+  },
   "CrmClientId": "...",
   "CrmClientSecret": "...",
   "CrmUrl": "..."
 }
```


## Formatting

Pull request builds will run format checks on .NET and Terraform code changes; if there are any issues the build will fail.

Before committing you can format any changed files by running:
```shell
just format-changed
```

To format the entire codebase run
```shell
just format
```
