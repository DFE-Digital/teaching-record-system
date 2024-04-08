set shell := ["pwsh", "-nop", "-c"]

solution-root := "TeachingRecordSystem"
user-secrets-id := "TeachingRecordSystem"
test-user-secrets-id := "TeachingRecordSystemTests"

default:
  @just --list

# Install local tools
install-tools:
  @cd {{solution-root}} && dotnet tool restore
  npm install -g sass

# Restore dependencies
restore:
  @cd {{solution-root}} && dotnet restore
  @cd {{solution-root / "src" / "TeachingRecordSystem.SupportUi" }} && dotnet libman restore --verbosity quiet
  @cd {{solution-root / "src" / "TeachingRecordSystem.AuthorizeAccess" }} && dotnet libman restore --verbosity quiet

# Run the trscli
cli *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Cli"}} && dotnet {{"bin" / "Debug" / "net8.0" / "trscli.dll"}} {{ARGS}}

# Build the .NET solution
build:
  @cd {{solution-root}} && dotnet build

# Test the .NET solution
test:
  @cd {{solution-root}} && dotnet test

# Format the .NET solution and Terraform code
format:
  @cd {{solution-root}} && dotnet format
  @terraform fmt terraform/aks

# Format any un-committed .tf or .cs files
format-changed:
  #!pwsh -nop

  function Get-ChangedFiles {
    param (
      $Path
    )

    (git status --porcelain $Path) | foreach { $_.substring(3) } | Where-Object { Test-Path $_ }
  }

  $changedTfFiles = Get-ChangedFiles "terraform/*.tf"
  foreach ($tf in $changedTfFiles) {
    terraform fmt $tf
  }

  $changedCsFiles = (Get-ChangedFiles "{{solution-root}}/**/*.cs") | foreach { $_ -Replace "^{{solution-root}}/", "" }
  if ($changedCsFiles.Length -gt 0) {
    $dotnetArgs = @("format", "--no-restore", "--include") + $changedCsFiles
    cd {{solution-root}} && dotnet $dotnetArgs
  }

# Run the EF Core Command-line Tools for the Core project
ef *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Core"}} && dotnet dotnet-ef {{ARGS}}

# Run the API project in Development mode
run-api:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet run

# Run the API project in Development mode and watch for file changes
watch-api:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet watch

# Run the AuthorizeAccess project in Development mode and watch for file changes
watch-authz:
  @cd {{solution-root / "src" / "TeachingRecordSystem.AuthorizeAccess"}} && dotnet watch

# Run the UI project in Development mode and watch for file changes
watch-ui:
  @cd {{solution-root / "src" / "TeachingRecordSystem.SupportUi"}} && dotnet watch

# Run the Worker project in Development mode and watch for file changes
watch-worker:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Worker"}} && dotnet watch

# Build the Docker image
docker-build *ARGS: install-tools restore
  @cd {{solution-root}} && dotnet publish -c Release --no-restore
  @cd {{solution-root}} && docker build . {{ARGS}}

# Set a configuration entry in user secrets for running the apps
set-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{user-secrets-id}}

# Set a configuration entry in user secrets for tests
set-tests-secret key value:
  @dotnet user-secrets set "{{key}}" "{{value}}" --id {{test-user-secrets-id}}

create-admin email name:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Cli"}} && dotnet {{"bin" / "Debug" / "net8.0" / "trscli.dll"}} create-admin --email {{email}} --name {{quote(name)}}

make *ARGS:
  @make {{ARGS}}

# Generates CRM model types
generate-crm-models *ARGS:
  @scripts/Generate-CrmModels.ps1 {{ARGS}}

# Removes the cached DB schema version file for tests
remove-tests-schema-cache:
  @scripts/Remove-TestsSchemaCache.ps1
