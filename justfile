set windows-shell := ["powershell.exe", "-nop", "-c"]

export DOTNET_WATCH_SUPPRESS_MSBUILD_INCREMENTALISM := 'true'
export DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER := 'true'
export DOTNET_WATCH_RESTART_ON_RUDE_EDIT := 'true'

shebang := if os() == 'windows' {
  'dotnet pwsh -nop'
} else {
  '/usr/bin/env dotnet pwsh -nop'
}

set working-directory := 'TeachingRecordSystem'

user-secrets-id := "TeachingRecordSystem"
test-user-secrets-id := "TeachingRecordSystemTests"

default:
  @just --list

# Install local tools
install-tools:
  @dotnet tool restore

# Restore dependencies
restore:
  @dotnet restore --locked-mode

# Install Playwright
[working-directory: 'tests/TeachingRecordSystem.AuthorizeAccess.EndToEndTests']
install-playwright:
  @pwsh bin/Debug/net9.0/playwright.ps1 install chromium

# Run the trscli
cli *ARGS:
  @dotnet {{"src" / "TeachingRecordSystem.Cli" / "bin" / "Debug" / "net9.0" / "trscli.dll"}} {{ARGS}}

# Build the .NET solution
build:
  @dotnet build

# Test the .NET solution
test:
  @dotnet test

# Format the .NET solution and Terraform code
format:
  @dotnet dotnet-format
  @terraform fmt ../terraform/aks

# Format any un-committed .tf or .cs files
format-changed:
  #!{{shebang}}

  function Get-ChangedFiles {
    param (
      $Path
    )

    (git status --porcelain $Path) | foreach { $_.substring(3) } | Where-Object { Test-Path $_ }
  }

  cd ../

  $changedTfFiles = Get-ChangedFiles "terraform/*.tf"
  foreach ($tf in $changedTfFiles) {
    terraform fmt $tf
  }

  $changedCsFiles = (Get-ChangedFiles "TeachingRecordSystem/**/*.cs") | foreach { $_ -Replace "^TeachingRecordSystem/", "" }
  if ($changedCsFiles.Length -gt 0) {
    $dotnetArgs = @("dotnet-format", "--no-restore", "--include") + $changedCsFiles
    cd TeachingRecordSystem && dotnet $dotnetArgs
  }

# Run the EF Core Command-line Tools for the Core project
[working-directory: 'src/TeachingRecordSystem.Core']
ef *ARGS:
  @dotnet dotnet-ef {{ARGS}}

# Run the API project in Development mode
[working-directory: 'src/TeachingRecordSystem.Api']
run-api:
  @dotnet run

# Run the API project in Development mode and watch for file changes
[working-directory: 'src/TeachingRecordSystem.Api']
watch-api:
  @dotnet watch

# Run the AuthorizeAccess project in Development mode and watch for file changes
[working-directory: 'src/TeachingRecordSystem.AuthorizeAccess']
watch-authz:
  @dotnet watch

# Run the UI project in Development mode and watch for file changes
[working-directory: 'src/TeachingRecordSystem.SupportUi']
watch-ui:
  @dotnet watch

# Watch for file changes and compile any SASS files that have changed
[working-directory: 'src/TeachingRecordSystem.SupportUi']
watch-ui-sass:
  @sass wwwroot/Styles/site.scss wwwroot/Styles/site.css --watch

# Run the Worker project in Development mode and watch for file changes
[working-directory: 'src/TeachingRecordSystem.Worker']
watch-worker:
  @dotnet watch

# Build the Docker image
docker-build *ARGS: restore
  @dotnet publish -c Release --no-restore
  @docker build . {{ARGS}}

# Set a configuration entry in user secrets for running the apps
set-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{user-secrets-id}}

# Set a configuration entry in user secrets for tests
set-tests-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{test-user-secrets-id}}

[working-directory: 'src/TeachingRecordSystem.Cli']
create-admin email name:
  @dotnet {{"bin" / "Debug" / "net9.0" / "trscli.dll"}} create-admin --email {{email}} --name {{quote(name)}}

[working-directory: '..']
make *ARGS:
  @make {{ARGS}}

# Generates CRM model types
generate-crm-models *ARGS:
  @dotnet pwsh -nop -file ../scripts/Generate-CrmModels.ps1 {{ARGS}}

# Removes the cached DB schema version file for tests
remove-tests-schema-cache:
  @dotnet pwsh -nop -file ../scripts/Remove-TestsSchemaCache.ps1
