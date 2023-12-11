# teaching-record-system

Provides an API over the Database of Qualified Teachers (DQT). Provides the core Teaching record and will eventually replace the Database of Qualified Teachers (DQT).
[trs containers diagram](docs/c4-diagrams-as-code/trs-containers.png)


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

Install Postgres then set a connection string configuration entry in user secrets for running the apps and another for running the tests.
Note you should use a different database for tests as the test database will be cleared down whenever tests are run.

e.g.
```shell
just set-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs"
just set-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=trs_tests"
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
just set-tests-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReportingTests;Integrated Security=Yes;TrustServerCertificate=True"
```

To run the service locally, override the configuration option to run the service and ensure a connection string is provided e.g.
```shell
just set-secret DqtReporting:RunService true
just set-secret DqtReporting:ReportingDbConnectionString "Data Source=(local);Initial Catalog=DqtReporting;Integrated Security=Yes;TrustServerCertificate=True"
```
The service will now run as a background service of the `Worker` project.


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
