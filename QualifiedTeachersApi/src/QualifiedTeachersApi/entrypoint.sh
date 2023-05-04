#!/bin/bash

set -e

alias qtcli="dotnet /QtCli/QtCli.dll"

if [ "$CF_INSTANCE_INDEX" == "0" ]; then
  HOST=$(echo "$VCAP_SERVICES" | jq -r '.postgres[0].credentials.host')
  DATABASE=$(echo "$VCAP_SERVICES" | jq -r '.postgres[0].credentials.name')
  USERNAME=$(echo "$VCAP_SERVICES" | jq -r '.postgres[0].credentials.username')
  PASSWORD=$(echo "$VCAP_SERVICES" | jq -r '.postgres[0].credentials.password')
  PORT=$(echo "$VCAP_SERVICES" | jq -r '.postgres[0].credentials.port')

  CONNECTION_STRING="Host=$HOST;Database=$DATABASE;Username=$USERNAME;Password='$PASSWORD';Port=$PORT;SslMode=Require;TrustServerCertificate=true"

  echo "Applying database migrations..."
  dotnet /QtCli/QtCli.dll migrate-db --connection-string "$CONNECTION_STRING"
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
