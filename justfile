set shell := ["pwsh", "-nop", "-c"]

solution-root := "TeachingRecordSystem"

default:
  @just --list

# Run the QtCli
cli *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Cli"}} && dotnet {{"bin" / "Release" / "net7.0" / "trscli.dll"}} {{ARGS}}

# Build the .NET solution
build:
  @cd {{solution-root}} && dotnet build

# Test the .NET solution
test:
  @cd {{solution-root}} && dotnet test

# Format the .NET solution and Terraform code
format:
  @cd {{solution-root}} && dotnet format
  @just tf fmt

# Format any un-committed .tf or .cs files
format-changed:
  #!pwsh -nop

  $changedTfFiles = & git diff HEAD --name-only "terraform/*.tf"
  foreach ($tf in $changedTfFiles) {
    terraform fmt $tf
  }

  $changedCsFiles = (& git diff HEAD --name-only "{{solution-root}}/**/*.cs") -Replace "^{{solution-root}}/", ""
  if ($changedCsFiles.Length -gt 0) {
    $dotnetArgs = @("format", "--no-restore", "--include") + $changedCsFiles
    cd {{solution-root}} && dotnet $dotnetArgs
  }

# Run the EF Core Command-line Tools for the Api project
ef *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet ef {{ARGS}}

# Run the API project in Development mode
run-api:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet run

# Run the API project in Development mode and watch for file changes
watch-api:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet watch

# Build the API Docker image
docker-build *ARGS:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet publish -c Release
  @cd {{solution-root / "src" / "TeachingRecordSystem.Cli"}} && dotnet publish -c Release
  @cd {{solution-root}} && docker build . -f {{"src" / "TeachingRecordSystem.Api" / "Dockerfile"}} {{ARGS}}

# Set a configuration entry in user secrets for the API project
set-api-secret key value:
  @cd {{solution-root / "src" / "TeachingRecordSystem.Api"}} && dotnet user-secrets set "{{key}}" "{{value}}"

# Set a configuration entry in user secrets for the API tests project
set-api-tests-secret key value:
  @cd {{solution-root / "tests" / "TeachingRecordSystem.Api.Tests"}} && dotnet user-secrets set "{{key}}" "{{value}}"

# Run terraform from the terraform directory
tf *ARGS:
  @cd "terraform" && terraform {{ARGS}}

make *ARGS:
  @make {{ARGS}}

# Generates CRM model types
generate-crm-models *ARGS:
  @scripts/Generate-CrmModels.ps1 {{ARGS}}
