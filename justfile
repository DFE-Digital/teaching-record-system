set shell := ["pwsh", "-c"]

solution-root := "QualifiedTeachersApi"

default:
  @just --list

# Run the QtCli
cli *ARGS:
  @cd {{solution-root / "src" / "QtCli"}} && dotnet {{"bin" / "Release" / "net7.0" / "QtCli.dll"}} {{ARGS}}

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

# Run the EF Core Command-line Tools for the QualifiedTeachersApi project
ef *ARGS:
  @cd {{solution-root / "src" / "QualifiedTeachersApi"}} && dotnet ef {{ARGS}}

# Run the API project in Development mode
run-api:
  @cd {{solution-root / "src" / "QualifiedTeachersApi"}} && dotnet run

# Run the API project in Development mode and watch for file changes
watch-api:
  @cd {{solution-root / "src" / "QualifiedTeachersApi"}} && dotnet watch

# Build the API Docker image
docker-build:
  @cd {{solution-root / "src" / "QualifiedTeachersApi"}} && dotnet publish -c Release
  @cd {{solution-root / "src" / "QtCli"}} && dotnet publish -c Release
  @cd {{solution-root}} && docker build . -f {{"src" / "QualifiedTeachersApi" / "Dockerfile"}}

# Set a configuration entry in user secrets for the API project
set-api-secret key value:
  @cd {{solution-root / "src" / "QualifiedTeachersApi"}} && dotnet user-secrets set "{{key}}" "{{value}}"

# Set a configuration entry in user secrets for the API tests project
set-api-tests-secret key value:
  @cd {{solution-root / "tests" / "QualifiedTeachersApi.Tests"}} && dotnet user-secrets set "{{key}}" "{{value}}"

# Run terraform from the terraform directory
tf *ARGS:
  @cd "terraform" && terraform {{ARGS}}

make *ARGS:
  @make {{ARGS}}

# Generates CRM model types
generate-crm-models *ARGS:
  @scripts/Generate-CrmModels.ps1 {{ARGS}}

# Synchronize the API specs against the API
sync-api-specs *ARGS:
  @scripts/Sync-ApiSpecs {{ARGS}}
