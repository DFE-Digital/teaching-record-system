export DOTNET_WATCH_SUPPRESS_MSBUILD_INCREMENTALISM := 'true'
export DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER := 'true'
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT := 'true'

set windows-shell := ["powershell.exe", "-nop", "-c"]

user-secrets-id := "TeachingRecordSystem"
test-user-secrets-id := "TeachingRecordSystemTests"

default:
  @just --list

# Install local tools
[working-directory: 'TeachingRecordSystem']
install-tools:
  @dotnet tool restore

# Restore dependencies
[working-directory: 'TeachingRecordSystem']
restore:
  @dotnet restore --locked-mode

# Install Playwright
[working-directory: 'TeachingRecordSystem/tests/TeachingRecordSystem.AuthorizeAccess.EndToEndTests']
install-playwright:
  @pwsh -nop -c bin/Debug/net10.0/playwright.ps1 install chromium

# Run the trscli
cli *ARGS:
  @dotnet {{"TeachingRecordSystem" / "src" / "TeachingRecordSystem.Cli" / "bin" / "Debug" / "net10.0" / "trscli.dll"}} {{ARGS}}

# Build the .NET solution
[working-directory: 'TeachingRecordSystem']
build:
  @dotnet build

# Test the .NET solution
[working-directory: 'TeachingRecordSystem']
test:
  @dotnet test

# Test projects affected by changes from main branch
test-changed *ARGS:
  @dotnet run scripts/TestChanged.cs -- {{ARGS}}

# Format the .NET solution and Terraform code
[working-directory: 'TeachingRecordSystem']
format:
  @dotnet format --exclude src/TeachingRecordSystem.Core/DataStore/Postgres/Migrations
  @terraform fmt ../terraform/aks

# Format any un-committed .tf or .cs files
format-changed:
  @dotnet run scripts/FormatChanged.cs

# Run the EF Core Command-line Tools for the Core project
[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.Core']
ef *ARGS:
  @dotnet dotnet-ef {{ARGS}}

# Run the API project in Development mode
[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.Api']
run-api:
  @dotnet run

# Run the API project in Development mode and watch for file changes
[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.Api']
watch-api:
  @dotnet watch

# Run the AuthorizeAccess project in Development mode and watch for file changes
[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.AuthorizeAccess']
watch-authz:
  @dotnet watch

# Run the UI project in Development mode and watch for file changes
[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.SupportUi']
watch-ui:
  @dotnet watch

# Watch for file changes and compile any SASS files that have changed
[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.SupportUi']
watch-ui-sass:
  @sass wwwroot/Styles/site.scss wwwroot/Styles/site.css --watch

# Run the Worker project in Development mode and watch for file changes
[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.Worker']
watch-worker:
  @dotnet watch

# Build the Docker image
[working-directory: 'TeachingRecordSystem']
docker-build *ARGS: restore
  @dotnet publish -c Release --no-restore
  @docker build . {{ARGS}}

# Set a configuration entry in user secrets for running the apps
[working-directory: 'TeachingRecordSystem']
set-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{user-secrets-id}}

# Set a configuration entry in user secrets for tests
[working-directory: 'TeachingRecordSystem']
set-tests-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{test-user-secrets-id}}

[working-directory: 'TeachingRecordSystem/src/TeachingRecordSystem.Cli']
create-admin email name:
  @dotnet {{"bin" / "Debug" / "net10.0" / "trscli.dll"}} create-admin --email {{email}} --name {{quote(name)}}

[working-directory: '..']
make *ARGS:
  @make {{ARGS}}

deploy-dev branch=`git branch --show-current`: (_deploy branch 'dev')

deploy-preprod branch=`git branch --show-current`: (_deploy branch 'pre-production')

[private]
_deploy branch environment:
  @gh workflow run deploy.yml --ref {{branch}} -f environment={{environment}}

# Removes the cached DB schema version file for tests
remove-tests-schema-cache:
  @dotnet run scripts/RemoveTestsSchemaCache.cs
