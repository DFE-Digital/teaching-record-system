FROM mcr.microsoft.com/dotnet/sdk:10.0.302-alpine3.23@sha256:d8ee39817ca03a3757288e83c37ed73cc969a286c603b827c7cbe33add1c2d1c AS build
WORKDIR /source

# Install pre-reqs for SASS compilation
RUN apk add --no-cache gcompat

COPY . ./
RUN dotnet tool restore
RUN dotnet restore
RUN dotnet publish --no-restore --configuration Release
RUN dotnet ef migrations bundle -p src/TeachingRecordSystem.Core/ --configuration Release --context TrsDbContext --output efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.23@sha256:1201dde897ab436b7c6b386f6dbd4f9a3ca0245f9c5a8aac8f8bcdccb4c7d484
ARG GIT_SHA
WORKDIR /Apps
COPY --from=build /source/src/TeachingRecordSystem.Api/bin/Release/net10.0/publish/ ./Api/
COPY --from=build /source/src/TeachingRecordSystem.Cli/bin/Release/net10.0/publish/ ./TrsCli/
COPY --from=build /source/src/TeachingRecordSystem.SupportUi/bin/Release/net10.0/publish/ ./SupportUi/
COPY --from=build /source/src/TeachingRecordSystem.Worker/bin/Release/net10.0/publish/ ./Worker/
COPY --from=build /source/src/TeachingRecordSystem.AuthorizeAccess/bin/Release/net10.0/publish/ ./AuthorizeAccess/
COPY --from=build /source/efbundle ./efbundle

# Ensure culture data is available
RUN apk add --no-cache tzdata icu-data-full icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk --no-cache add postgresql17-client

# Fix for invoking trscli
RUN apk --no-cache add libc6-compat

ENV SENTRY_RELEASE=${GIT_SHA}
ENV GIT_SHA=${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS=3000
ENV PATH="${PATH}:/Apps/TrsCli"

RUN addgroup -S appgroup -g 20001 && adduser -S appuser -G appgroup -u 10001

USER 10001
