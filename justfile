set shell := ["pwsh", "-nop", "-c"]

solution-root := "TeachingRecordSystem"

default:
  @just --list

# Install .NET local tools
install-tools:
  @cd {{solution-root}} && dotnet tool restore

# Run the trscli
cli *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Cli"}} && dotnet {{"bin" / "Release" / "net7.0" / "trscli.dll"}} {{ARGS}}

# Build the .NET solution
build:
  @cd {{solution-root}} && dotnet build

# Test the .NET solution
test:
  @cd {{solution-root / "tests" / "TeachingRecordSystem.Core.Tests"}} && dotnet fixie
  @cd {{solution-root}} && dotnet test

# Format the .NET solution and Terraform code
format:
  @cd {{solution-root}} && dotnet format
  @terraform fmt terraform/aks
  @terraform fmt terraform/paas

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
  @cd {{solution-root / "src" / "TeachingRecordSystem.Core"}} && dotnet ef {{ARGS}}

# Run the API project in Development mode
run-api:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet run

# Run the API project in Development mode and watch for file changes
watch-api:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet watch

# Run the UI project in Development mode and watch for file changes
watch-ui:
  @cd {{solution-root / "src" / "TeachingRecordSystem.SupportUi"}} && dotnet watch

# Build the API Docker image
docker-build-api *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet publish -c Release
  @cd {{solution-root / "src" / "TeachingRecordSystem.Cli"}} && dotnet publish -c Release
  @cd {{solution-root}} && docker build . -f {{"src" / "TeachingRecordSystem.Api" / "Dockerfile"}} {{ARGS}}

# Build the CLI Docker image
docker-build-cli *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Cli"}} && dotnet publish -c Release
  @cd {{solution-root}} && docker build . -f {{"src" / "TeachingRecordSystem.Cli" / "Dockerfile"}} {{ARGS}}

# Build the Support UI Docker image
docker-build-ui *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.SupportUi"}} && dotnet publish -c Release
  @cd {{solution-root}} && docker build . -f {{"src" / "TeachingRecordSystem.SupportUi" / "Dockerfile"}} {{ARGS}}

# Set a configuration entry in user secrets for the API project
set-api-secret key value:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet user-secrets set "{{key}}" "{{value}}"

# Set a configuration entry in user secrets for the API tests project
set-api-tests-secret key value:
  @cd {{solution-root / "tests" / "TeachingRecordSystem.Api.Tests"}} && dotnet user-secrets set "{{key}}" "{{value}}"

# Set a configuration entry in user secrets for the DQT tests project
set-dqt-tests-secret key value:
  @cd {{solution-root / "tests" / "TeachingRecordSystem.Dqt.Tests"}} && dotnet user-secrets set "{{key}}" "{{value}}"

make *ARGS:
  @make {{ARGS}}

# Generates CRM model types
generate-crm-models *ARGS:
  @scripts/Generate-CrmModels.ps1 {{ARGS}}
