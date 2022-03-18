# syntax=docker/dockerfile:1
  FROM mcr.microsoft.com/dotnet/aspnet:6.0
  ARG GIT_SHA
  ENV GitSha ${GIT_SHA}
  COPY src/DqtApi/bin/Release/net6.0/publish/ App/
  WORKDIR /App
  ENTRYPOINT ["dotnet", "DqtApi.dll"]
  EXPOSE 80
