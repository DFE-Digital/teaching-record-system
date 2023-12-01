# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY src/TeachingRecordSystem.Cli/bin/Release/net8.0/publish/ TrsCli/
WORKDIR /TrsCli
ENV PATH="${PATH}:/TrsCli"
