# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RailwayExpressVN.ServiceDefaults/RailwayExpressVN.ServiceDefaults.csproj", "RailwayExpressVN.ServiceDefaults/"]
COPY ["src/Common/Common.API/Common.API.csproj", "src/Common/Common.API/"]
COPY ["src/Common/Common.Application/Common.Application.csproj", "src/Common/Common.Application/"]
COPY ["src/Common/Common.Contracts/Common.Contracts.csproj", "src/Common/Common.Contracts/"]
COPY ["src/Common/Common.Domain/Common.Domain.csproj", "src/Common/Common.Domain/"]
COPY ["src/Common/Common.Infrastructure/Common.Infrastructure.csproj", "src/Common/Common.Infrastructure/"]

COPY ["src/Services/Authentication/Auth.API/Auth.API.csproj", "src/Services/Authentication/Auth.API/"]
COPY ["src/Services/Authentication/Auth.Application/Auth.Application.csproj", "src/Services/Authentication/Auth.Application/"]
RUN dotnet restore "src/Services/Authentication/Auth.API/Auth.API.csproj"

COPY . .
WORKDIR "//src/src/Services/Authentication/Auth.API"
RUN dotnet build "Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]