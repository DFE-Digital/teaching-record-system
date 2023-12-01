# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.18
ARG GIT_SHA
ENV GitSha ${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS 80
COPY src/TeachingRecordSystem.SupportUi/bin/Release/net8.0/publish/ App/
WORKDIR /App

# Install Culture prerequisities
RUN apk add --no-cache \
        tzdata \
        icu-data-full \
        icu-libs

# Enable all cultures
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "/App/TeachingRecordSystem.SupportUi.dll"]
EXPOSE 80
