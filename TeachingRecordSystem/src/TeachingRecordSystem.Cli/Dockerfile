# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine3.18
COPY src/TeachingRecordSystem.Cli/bin/Release/net7.0/publish/ TrsCli/
WORKDIR /TrsCli
ENV PATH="${PATH}:/TrsCli"
