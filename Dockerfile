FROM mcr.microsoft.com/dotnet/sdk:11.0.100-alpine3.24@sha256:52556c0fb9d0f609ae9aeab9efeeba96f4acc2f7038f7696f2235ccbdbb8e290 AS build
WORKDIR /source

# Install pre-reqs for SASS compilation
RUN apk add --no-cache gcompat

COPY . ./
RUN dotnet tool restore
RUN dotnet restore
RUN dotnet publish --no-restore --configuration Release
RUN dotnet ef migrations bundle -p src/TeachingRecordSystem.Core/ --configuration Release --context TrsDbContext --output efbundle

FROM mcr.microsoft.com/dotnet/aspnet:11.0-alpine3.24@sha256:d39b72ad1534fa0faf52b488bffb0762df7ba269d120f0338713d4dda2a41bf5
ARG GIT_SHA
WORKDIR /Apps
COPY --from=build /source/src/TeachingRecordSystem.Api/bin/Release/net11.0/publish/ ./Api/
COPY --from=build /source/src/TeachingRecordSystem.Cli/bin/Release/net11.0/publish/ ./TrsCli/
COPY --from=build /source/src/TeachingRecordSystem.SupportUi/bin/Release/net11.0/publish/ ./SupportUi/
COPY --from=build /source/src/TeachingRecordSystem.Worker/bin/Release/net11.0/publish/ ./Worker/
COPY --from=build /source/src/TeachingRecordSystem.AuthorizeAccess/bin/Release/net11.0/publish/ ./AuthorizeAccess/
COPY --from=build /source/efbundle ./efbundle

# Ensure culture data is available
RUN apk add --no-cache tzdata icu-data-full icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN apk --no-cache add postgresql17-client

# Fix for invoking trscli
RUN apk --no-cache add libc6-compat

# Pick up security fixes not yet in the base image (e.g. zlib CVE fixed in 1.3.2-r1)
RUN apk --no-cache upgrade zlib

ENV SENTRY_RELEASE=${GIT_SHA}
ENV GIT_SHA=${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS=3000
ENV PATH="${PATH}:/Apps/TrsCli"

RUN addgroup -S appgroup -g 20001 && adduser -S appuser -G appgroup -u 10001

USER 10001
