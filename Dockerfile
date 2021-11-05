# syntax=docker/dockerfile:1
  FROM mcr.microsoft.com/dotnet/aspnet:5.0
  COPY src/DqtApi/bin/Release/net5.0/publish/ App/
  WORKDIR /App
  ENTRYPOINT ["dotnet", "DqtApi.dll"]
  EXPOSE 80
