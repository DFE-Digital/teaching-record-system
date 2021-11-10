# Running functional tests

The functional test project is at `tests/DqtApi.FunctionalTests/`.

When run in development mode (e.g. from Visual Studio) the project will launch the main API project (`src/DqtApi/`) and use that instance as the target for the tests i.e. `http://localhost:5000`.

Secrets and test data come from [User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows) with ID `DqtApiFunctionalTests`. Ask a developer for a copy of the secrets.

## Running tests against the EAPIM implementation

There is a PowerShell script that will run a subset of the functional tests against the legacy API implementation hosted on EAPIM. The script is at `tests/DqtApi.FunctionalTests/TestApim.ps1`.

The script requires endpoints and test data from User Secrets with ID `DqtApiApimIntegrationTests`. Ask a developer for a copy of the secrets.
