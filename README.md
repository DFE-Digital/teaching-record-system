# qualified-teachers-api

[![Build](https://github.com/DFE-Digital/qualified-teachers-api/actions/workflows/build.yml/badge.svg)](https://github.com/DFE-Digital/qualified-teachers-api/actions/workflows/build.yml)

Provides a RESTful API for integrating with the Database of Qualified Teachers CRM.


## Developer setup

### Software requirements

The API is an ASP.NET Core 7 web application. To develop locally you will need the following installed:
- Visual Studio 2022 (or the .NET 7 SDK an alternative IDE/editor);
- a local Postgres 13+ instance.

A `justfile` defines various recipes for development. Ensure [just](https://just.systems/) is installed and available on your `$PATH`.

### Database setup

Install Postgres then set a connection string configuration entry in user secrets for both the `QualifiedTeachersApi` and `QualifiedTeachersApi.Tests` projects.

e.g.
```shell
just set-api-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=dqt"
just set-api-tests-secret ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=dqt_tests"
```

The databases will be created automatically when running the API or tests in development mode.

### External dependencies

#### Dynamics CRM

The `build` CRM environment is used for local development and automated tests. Connection information should be stored in user secrets in the `CrmUrl`, `CrmClientId` and `CrmClientSecret` keys. Secrets must be added for both the API project and the API tests project. Secrets can be added set a `just` recipe e.g.
```shell
just set-api-secret CrmUrl "https://the-crm-environment-url"
just set-api-secret CrmClientId "the-client-id"
just set-api-secret CrmClientSecret "the-client-secret"
just set-api-tests-secret CrmUrl "https://the-crm-environment-url"
just set-api-tests-secret CrmClientId "the-client-id"
just set-api-tests-secret CrmClientSecret "the-client-secret"
```
Ask a developer on the team for the user secrets for this environment.

#### TRN Generation API

The DQT API calls a TRN Generation API to generate a TRNs. Configuration should be stored in user secrets in the `TrnGenerationApi:BaseAddress` and `TrnGeneration:ApiKey` keys. Secrets must be added for both the API project and the API tests project. Secrets can be set using a `just` recipe e.g.
```shell
just set-api-secret TrnGenerationApi:BaseAddress "https://the-trn-generation-url"
just set-api-secret TrnGenerationApi:ApiKey "the-api-key"
just set-api-tests-secret TrnGenerationApi:BaseAddress "https://the-trn-generation-url"
just set-api-tests-secret TrnGenerationApi:ApiKey "the-api-key"
```
Ask a developer on the team for the user secrets for this environment.


## CRM code generation

A tool is used to generated proxy classes for the entities defined within the DQT CRM.
The tool generates the `QualifiedTeachersApi.DataStore.Crm.Models.GeneratedCode.cs` and `QualifiedTeachersApi.DataStore.Crm.Models.GeneratedOptionSets.cs` files.
A configuration file at `crm_attributes.json` whitelists the entities and their attributes which are included on the generated types.

Run `just generate-crm-models` to run the code generator against the `build` environment using the configuration file above.
The CRM user secrets described within [Developer setup](#dynamics-crm) must be correctly set for the tool to run successfully.
The tool is a .NET Framework application and requires .NET 4.6.


## API specs

All endpoints exposed by the API are versioned - `v1`, `v2` etc. Each version has its own Swagger doc describing the endpoints within that version.
The specs are stored in `docs/api-specs`. These specs are published to a docs site during the CI process.

These specs are generated from a deployed instance of the API using `just sync-api-specs`. By default the script looks at https://localhost:5001. To override this location provide the `BaseUrl` parameter e.g. `just sync-api-specs -BaseUrl http://localhost:1234`.

A step in the build pipeline verifies that the specs are in-sync with the implementation. The build will fail if there are discrepancies.


## Environment configuration

Environment-specific configuration is stored in Key Vault inside a single JSON-encoded key named 'APP-CONFIG'.
This is retrieved via Terraform at deployment time and is set as an environment variable so it can be accessed by the application.

A helper script is provided to simplify adding or amending configuration for a specific environment - `Set-AppConfigSecret.ps1`.
For this to run successfully you will need the Azure CLI. You need to be authenticated to access the s165 subscriptions before running the script.

By default the script runs read-only; it calculates the new JSON configuration and print out the result to the console. To commit the change add the `-Commit` switch.

### Example - adding an API client

```shell
./Set-AppConfigSecret -EnvironmentName dev -AzureSubscription s165-teachingqualificationsservice-development -ConfigKey ApiClients:new_client:ApiKey -ConfigValue super-secret-key
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

## More information

- [Environment setup](docs/environment-setup.md)
- [Running load tests](docs/running-load-tests.md)
