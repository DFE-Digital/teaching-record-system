#!/bin/bash

set -e

alias qtcli="dotnet /QtCli/QtCli.dll"

if [ "$CF_INSTANCE_INDEX" == "0" ]; then
  echo "Applying database migrations..."
  dotnet /QtCli/QtCli.dll migrate-db --connection-string "$ConnectionStrings__DefaultConnection"
  echo "Done applying database migrations"

  REPORTING_DB_CONNSTR="$DqtReporting__ReportingDbConnectionString"

  if [ -z "$REPORTING_DB_CONNSTR" ]; then
    REPORTING_DB_CONNSTR=$(echo "$AppConfig" | jq -r '.DqtReporting.ReportingDbConnectionString')
  fi

  if [ ! -z "$REPORTING_DB_CONNSTR" ]; then
   echo "Applying reporting database migrations..."
   dotnet /QtCli/QtCli.dll migrate-reporting-db --connection-string "$REPORTING_DB_CONNSTR"
   echo "Done applying reporting database migrations"
 fi
fi

if [ -z "$REPORTING_DB_CONNSTR" ]; then
  echo "Disabling DqtReportingService"
  export DqtReporting__RunService="false"
fi

dotnet /App/QualifiedTeachersApi.dll
