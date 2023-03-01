# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:7.0
ARG GIT_SHA
ENV GitSha ${GIT_SHA}
COPY bin/Release/net7.0/publish/ App/
WORKDIR /App

# install required fonts for PdfSharpCore
RUN sed -i'.bak' 's/$/ contrib/' /etc/apt/sources.list
RUN apt-get update; apt-get install -y ttf-mscorefonts-installer fontconfig

ENTRYPOINT ["dotnet", "QualifiedTeachersApi.dll"]
EXPOSE 80
