# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:7.0
COPY src/TeachingRecordSystem.Cli/bin/Release/net7.0/publish/ TrsCli/
WORKDIR /TrsCli
ENV PATH="${PATH}:/TrsCli"
