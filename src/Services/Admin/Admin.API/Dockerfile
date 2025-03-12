# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Admin/Admin.API/Admin.API.csproj", "Services/Admin/Admin.API/"]
COPY ["Common/Common.API/Common.API.csproj", "Common/Common.API/"]
COPY ["Common/Common.Application/Common.Application.csproj", "Common/Common.Application/"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Infrastructure/Common.Infrastructure.csproj", "Common/Common.Infrastructure/"]
COPY ["Services/Admin/Admin.Application/Admin.Application.csproj", "Services/Admin/Admin.Application/"]
COPY ["Services/Admin/Admin.Domain/Admin.Domain.csproj", "Services/Admin/Admin.Domain/"]
COPY ["Services/Admin/Admin.Infrastructure/Admin.Infrastructure.csproj", "Services/Admin/Admin.Infrastructure/"]
RUN dotnet restore "./Services/Admin/Admin.API/Admin.API.csproj"
COPY . .
WORKDIR "/src/Services/Admin/Admin.API"
RUN dotnet build "./Admin.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Admin.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Admin.API.dll"]