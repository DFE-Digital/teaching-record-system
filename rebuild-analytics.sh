#!/usr/bin/env bash

set -euo pipefail

rm -rf TeachingRecordSystem/package_cache/dfe*
rm -rf TeachingRecordSystem/lib/*
mkdir -p TeachingRecordSystem/lib
fd packages.lock.json | xargs rm

(cd ../dfe-analytics-dotnet && rm -rf packages && dotnet pack -c Release)

cp -r ../dfe-analytics-dotnet/packages/DfeAnalytics.*.nupkg TeachingRecordSystem/lib

(cd TeachingRecordSystem && dotnet restore)
