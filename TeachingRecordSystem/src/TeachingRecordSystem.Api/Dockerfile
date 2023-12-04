# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.18
ARG GIT_SHA
ENV SENTRY_RELEASE ${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS 80
COPY src/TeachingRecordSystem.Api/bin/Release/net8.0/publish/ App/
COPY src/TeachingRecordSystem.Cli/bin/Release/net8.0/publish/ TrsCli/
WORKDIR /App

# Install Culture prerequisities
RUN apk add --no-cache \
        tzdata \
        icu-data-full \
        icu-libs

# Enable all cultures
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install fonts for PDF generation
RUN apk --no-cache add msttcorefonts-installer fontconfig && \
    update-ms-fonts && \
    fc-cache -f

ENV PATH="${PATH}:/TrsCli"

ENTRYPOINT ["dotnet", "/App/TeachingRecordSystem.Api.dll"]
EXPOSE 80
