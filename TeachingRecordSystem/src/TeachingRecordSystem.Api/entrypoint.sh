#!/bin/bash

set -e

CONNSTR=$(dotnet /App/TeachingRecordSystem.Api.dll config "ConnectionStrings:DefaultConnection")
REPORTING_DB_CONNSTR=$(dotnet /App/TeachingRecordSystem.Api.dll config "DqtReporting:ReportingDbConnectionString")

if [ "$CF_INSTANCE_INDEX" == "0" ]; then
  echo "Applying database migrations..."
  qtcli migrate-db --connection-string "$CONNSTR"
  echo "Done applying database migrations" 

  echo "Applying reporting database migrations..."
  qtcli migrate-reporting-db --connection-string "$REPORTING_DB_CONNSTR"
  echo "Done applying reporting database migrations"
else
  echo "Disabling DqtReportingService"
  export DqtReporting__RunService="false"
fi

dotnet /App/TeachingRecordSystem.Api.dll
