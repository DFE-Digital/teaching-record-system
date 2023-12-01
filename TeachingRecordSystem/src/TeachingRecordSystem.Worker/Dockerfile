# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine3.18
ARG GIT_SHA
ENV GitSha ${GIT_SHA}
COPY src/TeachingRecordSystem.Worker/bin/Release/net8.0/publish/ App/
COPY src/TeachingRecordSystem.Cli/bin/Release/net8.0/publish/ TrsCli/
WORKDIR /App

# Install Culture prerequisities
RUN apk add --no-cache \
        tzdata \
        icu-data-full \
        icu-libs

# Enable all cultures
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "/App/TeachingRecordSystem.Worker.dll"]
