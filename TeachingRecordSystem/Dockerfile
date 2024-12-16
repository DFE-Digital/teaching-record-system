# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.18
ARG GIT_SHA
ENV SENTRY_RELEASE ${GIT_SHA}
ENV GIT_SHA ${GIT_SHA}
ENV ASPNETCORE_HTTP_PORTS 3000
COPY src/TeachingRecordSystem.Api/bin/Release/net8.0/publish/ Apps/Api/
COPY src/TeachingRecordSystem.Cli/bin/Release/net8.0/publish/ Apps/TrsCli/
COPY src/TeachingRecordSystem.SupportUi/bin/Release/net8.0/publish/ Apps/SupportUi/
COPY src/TeachingRecordSystem.Worker/bin/Release/net8.0/publish/ Apps/Worker/
COPY src/TeachingRecordSystem.AuthorizeAccess/bin/Release/net8.0/publish/ Apps/AuthorizeAccess/
COPY db.sh Apps/db.sh
WORKDIR /Apps

RUN chmod +x /Apps/db.sh

# Install Culture prerequisities
RUN apk add --no-cache \
        tzdata \
        icu-data-full \
        icu-libs

# Enable all cultures
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install Postgres client
RUN apk --no-cache add postgresql14-client

# Fix for invoking trscli
RUN apk --no-cache add libc6-compat

# Install fonts for PDF generation
RUN apk --no-cache add msttcorefonts-installer fontconfig && \
    update-ms-fonts && \
    fc-cache -f

# Install SQL Server tools needed to be able to query the reporting DB to help debugging
RUN apk add curl

RUN curl -O https://download.microsoft.com/download/b/9/f/b9f3cce4-3925-46d4-9f46-da08869c6486/msodbcsql18_18.0.1.1-1_amd64.apk && \
	curl -O https://download.microsoft.com/download/b/9/f/b9f3cce4-3925-46d4-9f46-da08869c6486/mssql-tools18_18.0.1.1-1_amd64.apk

RUN apk add --allow-untrusted msodbcsql18_18.0.1.1-1_amd64.apk && \
	apk add --allow-untrusted mssql-tools18_18.0.1.1-1_amd64.apk && \
	rm -f msodbcsql18_18.0.1.1-1_amd64.apk mssql-tools18_18.0.1.1-1_amd64.apk

ENV PATH="${PATH}:/Apps/TrsCli"
