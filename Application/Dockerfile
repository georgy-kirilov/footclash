FROM mcr.microsoft.com/dotnet/sdk:9.0 AS development
EXPOSE 8080
WORKDIR /src

ENV INSIDE_CONTAINER=True
ENV ASPNETCORE_ENVIRONMENT=Development

RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

CMD ["dotnet", "run", "--project", "./Application/Application.csproj", "-c", "Debug", "-v", "normal"]
