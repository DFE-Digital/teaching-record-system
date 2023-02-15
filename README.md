# qualified-teachers-api

[![Build](https://github.com/DFE-Digital/qualified-teachers-api/actions/workflows/build.yml/badge.svg)](https://github.com/DFE-Digital/qualified-teachers-api/actions/workflows/build.yml)

Provides a RESTful API for integrating with the Database of Qualified Teachers CRM.


## Developer setup

### Software requirements

The API is an ASP.NET Core 7 web application. To develop locally you will need the following installed:
- Visual Studio 2022 (or the .NET 7 SDK an alternative IDE/editor);
- a local Postgres 13+ instance.

### Database setup

Install Postgres then add a connection string to user secrets for both the `DqtApi` and `DqtApiTests` projects.

e.g.
```shell
dotnet user-secrets --id DqtApi set ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=dqt"
dotnet user-secrets --id DqtApiTests set ConnectionStrings:DefaultConnection "Host=localhost;Username=postgres;Password=your_postgres_password;Database=dqt_tests"
```

The databases will be created automatically when running the API or tests in development mode.

### External API setup

The DQT API can be configured to call a TRN Generation REST API to generate a TRN.
The is a feature which can be toggled on and off via config (it will default to being toggled off).
There are tests with this feature toggled on and off so it does not need to be set in config for tests.

```shell
dotnet user-secrets --id DqtApi set FeatureManagement:UseTrnGenerationApi true|false
dotnet user-secrets --id DqtApi set TrnGenerationApi:BaseAddress "base_address_for_trn_generation_api"
dotnet user-secrets --id DqtApi set TrnGenerationApi:ApiKey "api_key_for_trn_generation_api"
dotnet user-secrets --id DqtApiTests set TrnGenerationApi:BaseAddress "base_address_for_trn_generation_api"
dotnet user-secrets --id DqtApiTests set TrnGenerationApi:ApiKey "api_key_for_trn_generation_api"
```
Where `base_address_for_trn_generation_api` is the base address URL to access the TRN Generation API e.g. locally or deployed to Build/Dev.
Where `api_key_for_trn_generation_api` is an API Key to be able to access the TRN Generation API e.g. locally or deployed to Build/Dev.


### CRM connection

The `build` CRM environment is used for local development. Connection information is stored in user secrets in the `CrmUrl`, `CrmClientId` and `CrmClientSecret` keys.
Ask a developer on the team for the user secrets to connect to this environment.


## CRM code generation

A tool is used to generated proxy classes for the entities defined within the DQT CRM.
The tool generates the `DqtApi.DataStore.Crm.Models.GeneratedCode.cs` and `DqtApi.DataStore.Crm.Models.GeneratedOptionSets.cs` files.
A configuration file at `crm_attributes.json` whitelists the entities and their attributes which are included on the generated types.

The Powershell script `Generate-CrmModels.ps1` will re-run the code generator against the `build` environment using the configuration file above.
The CRM user secrets described within [Developer setup](#crm-connection) must be correctly set for the tool to run successfully.
The tool is a .NET Framework application and requires .NET 4.6.


## API specs

All endpoints exposed by the API are versioned - `v1`, `v2` etc. Each version has its own Swagger doc describing the endpoints within that version.
The specs are stored in `docs/api-specs`.

These specs are generated from the implementation using a script - `Sync-ApiSpecs.ps1`. By default the script looks at http://localhost:30473 - the location
that the API is hosted at in IIS for local development. To override this location provide the `BaseUrl` parameter e.g. `Sync-ApiSpecs.ps1 -BaseUrl http://localhost:5000`.

A step in the build pipeline verifies that the specs are in-sync with the implementation. The build will fail if there are differences.


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
- [Running functional tests](docs/running-functional-tests.md)
- [Running load tests](docs/running-load-tests.md)
