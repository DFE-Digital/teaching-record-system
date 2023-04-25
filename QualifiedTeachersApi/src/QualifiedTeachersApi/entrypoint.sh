#!/bin/bash

set -e

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
fi

dotnet /App/QualifiedTeachersApi.dll
