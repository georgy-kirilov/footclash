FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src
EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Development

COPY ["FootClash.sln", "./"]
COPY ["nuget.config", "./"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["UnitTests/UnitTests.csproj", "UnitTests/"]
COPY ["IntegrationTests/IntegrationTests.csproj", "IntegrationTests/"]
RUN dotnet restore "./FootClash.sln"
COPY . .

RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

FROM restore AS development
WORKDIR /src
RUN dotnet build "FootClash.sln" --no-restore -c Debug

CMD ["bash", "-c", "trap : TERM INT; sleep infinity & wait"]