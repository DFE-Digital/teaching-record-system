FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine3.23@sha256:732cd42c6f659814c9804ad7b05c7f761e83ef8379c5b2fdc3af673353caff73 AS build
WORKDIR /source

# Install pre-reqs for SASS compilation
RUN apk add --no-cache gcompat

COPY . ./
RUN dotnet tool restore
RUN dotnet restore
RUN dotnet publish --no-restore --configuration Release

FROM mcr.microsoft.com/dotnet/aspnet:10.0.8-noble-chiseled-extra-amd64@sha256:d3552fc1bd9b5195f6a397a547975fa1dbfb21870b4710f929eaa9adc5ceee42
ARG GIT_SHA
WORKDIR /Apps
COPY --from=build /source/src/TeachingRecordSystem.Api/bin/Release/net10.0/publish/ ./Api/
COPY --from=build /source/src/TeachingRecordSystem.Cli/bin/Release/net10.0/publish/ ./TrsCli/
COPY --from=build /source/src/TeachingRecordSystem.SupportUi/bin/Release/net10.0/publish/ ./SupportUi/
COPY --from=build /source/src/TeachingRecordSystem.Worker/bin/Release/net10.0/publish/ ./Worker/
COPY --from=build /source/src/TeachingRecordSystem.AuthorizeAccess/bin/Release/net10.0/publish/ ./AuthorizeAccess/

# Fix for invoking trscli
#RUN apk --no-cache add libc6-compat

ENV SENTRY_RELEASE=${GIT_SHA}
ENV GIT_SHA=${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS=3000
ENV PATH="${PATH}:/Apps/TrsCli"

USER app
