FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-amd64@sha256:69ff714a42f7931475acfcd2792d69cd4a656e4f3653d520e25c0fbe3c6d0cba AS build
WORKDIR /source

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

ENV SENTRY_RELEASE=${GIT_SHA}
ENV GIT_SHA=${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS=3000
ENV PATH="${PATH}:/Apps/TrsCli"

USER $APP_UID
