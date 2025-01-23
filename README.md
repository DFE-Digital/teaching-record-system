# teaching-record-system
The TRS is a database of people who work in Education in the UK. It is the primary source of teaching records for DfE. It holds data to meet UK Government statutory obligations as well as allow individuals working in the UK education system (including, but not limited to) teachers in schools within England, Wales, Scotland and Northern Ireland to access digital services provided by the DfE. It is also a DfE web service used by DfE support teams. It is the source of Teaching Reference Number (TRN) used in a number of Education services and processes both inside and outside of DfE.


## Teaching Reference Number
The TRN is a unique 7  digit reference given to identify a person who's data is in the Teaching Record System (formerly Database Of Qualified Teachers). TRN's are given to trainee teachers, qualified teachers (QTS and many other teaching related qualifications), anyone eligible for a teaching pension and other related services.

It is a character string of len(7). E.G:
```shell
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

| Name           | API URL |
| --- | --- |
| Production     | https://teacher-qualifications-api.education.gov.uk |
| Pre-production | https://preprod.teacher-qualifications-api.education.gov.uk |
| Test           | https://test.teacher-qualifications-api.education.gov.uk/ |
| Development    | https://dev.teacher-qualifications-api.education.gov.uk/ |


## Developer setup

### Software requirements

The API is an ASP.NET Core 8 web application. To develop locally you will need the following installed:
- Visual Studio 2022 (or the .NET 8 SDK and an alternative IDE/editor);
- a local Postgres 15+ instance;
- NPM.

A `justfile` defines various recipes for development. Ensure [just](https://just.systems/) is installed and available on your `$PATH` as well as [PowerShell](https://microsoft.com/PowerShell) v7+.

If you're working on infrastructure you will also need:
- make;
- Terraform;
- Azure CLI;
- bash.

### Local tools setup

A `just` recipe will install the required local tools:
```shell
just install-tools
```

### Restore dependencies

As well as NuGet packages, there are some client-side libraries required. A `just` recipe will install both:
```shell
just restore
```

### Blob storage emulator

The Support UI uses Azure Blob Storage for storing files. For local development, you can use the Azure Storage Emulator - Azurite. If you
use Visual Studio 2022 you probably have it installed already otherwise you can install it from [here](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite). Run it before starting the Support UI.

### Install Playwright

Playwright is used for end-to-end testing. Install it with a `just` recipe:
```shell
just install-playwright
```

### Database setup

Install Postgres then set a connection string configuration entry in user secrets for running the apps and another for running the tests.
Note you should use a different database for tests as the test database will be cleared down whenever tests are run.

e.g.
```shell
just set-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs"
just set-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
```

To set up the initial trs database schema run:
```shell
just build
just cli migrate-db
```

The trs_tests database will be created automatically when running the tests.

#### DQT Reporting database setup

This solution contains a service that synchronises changes from CRM into a SQL Server database used for reporting (this replaces the now-deprecated Data Export Service).
It also synchronises selected tables from TRS.
By default, this is disabled for local development. For the tests to pass, you will need a test database and a connection string defined in user secrets e.g.
```shell
just set-tests-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReportingTests;Integrated Security=Yes;TrustServerCertificate=True"
```

Your postgres server's `wal_level` must be set to `logical`:
```
ALTER SYSTEM SET wal_level = logical;
```
You will have to restart the server after amending this configuration.

To run the service locally override the configuration option to run the service and ensure a connection string is provided e.g.
```shell
just set-secret DqtReporting:RunService true
just set-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReporting;Integrated Security=Yes;TrustServerCertificate=True"
```
The service will now run as a background service of the `Worker` project.

It is a good idea to remove the replication slot when you're not working on this service to avoid a backlog on unprocessed changes accumulating in postgres.
```shell
just set-secret DqtReporting:RunService false
just cli drop-dqt-reporting-replication-slot
```


### Admin user setup

Add yourself to your local database as an administrator:
```shell
just create-admin "your.name@education.gov.uk" "Your Name"
```


### External dependencies

There are several external dependencies required for local development; these are listed below.
Ask a developer on the team for the user secrets for these dependencies.

#### Dynamics CRM

The `build` CRM environment is used for local development and automated tests.

#### TRN Generation API

The API calls the TRN Generation API to generate a TRNs.

#### Azure AD

Azure AD is used for authenticating users in the Support UI.


## CRM code generation

A tool is used to generated proxy classes for the entities defined within the DQT CRM.
The tool generates the `TeachingRecordSystem.Dqt.Models.GeneratedCode.cs` and `TeachingRecordSystem.Dqt.Models.GeneratedOptionSets.cs` files.
A configuration file at `crm_attributes.json` whitelists the entities and their attributes which are included on the generated types.

Run `just generate-crm-models` to run the code generator against the `build` environment using the configuration file above.
The CRM user secrets described within [Developer setup](#dynamics-crm) must be correctly set for the tool to run successfully.
The tool is a .NET Framework application and requires .NET 4.6.


## Environment configuration

Environment-specific configuration is stored in Key Vault inside a single JSON-encoded key named 'AppConfig'.
This is retrieved via Terraform at deployment time and is set as an environment variable so it can be accessed by the application.


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
