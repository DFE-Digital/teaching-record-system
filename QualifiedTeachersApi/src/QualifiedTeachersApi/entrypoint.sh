#!/bin/bash

set -e

alias qtcli="dotnet /QtCli/QtCli.dll"

if [ "$CF_INSTANCE_INDEX" == "0" ]; then
  echo "Applying database migrations..."
  dotnet /QtCli/QtCli.dll migrate-db --connection-string "$ConnectionStrings__DefaultConnection"
  echo "Done applying database migrations"

  if [ ! -z "$DqtReporting__ReportingDbConnectionString" ]; then
   echo "Applying reporting database migrations..."
   dotnet /QtCli/QtCli.dll migrate-reporting-db --connection-string "$DqtReporting__ReportingDbConnectionString"
   echo "Done applying reporting database migrations"
 fi
fi

if [ -z "$DqtReporting__ReportingDbConnectionString" ]; then
  echo "Disabling DqtReportingService"
  export DqtReporting__RunService="false"
fi

dotnet /App/QualifiedTeachersApi.dll
