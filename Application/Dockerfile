FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
EXPOSE 8080
WORKDIR /src/Application
ENV ASPNETCORE_ENVIRONMENT=Development
CMD ["dotnet", "run", "-c", "Debug", "-v", "normal"]
