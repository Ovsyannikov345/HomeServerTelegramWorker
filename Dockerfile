FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app

USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

USER app
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["HomeLabCore.Api/HomeLabCore.Api.csproj", "HomeLabCore.Api/"]
COPY ["HomeLabCore.Application/HomeLabCore.Application.csproj", "HomeLabCore.Application/"]
COPY ["HomeLabCore.Domain/HomeLabCore.Domain.csproj", "HomeLabCore.Domain/"]
COPY ["HomeLabCore.Infrastructure/HomeLabCore.Infrastructure.csproj", "HomeLabCore.Infrastructure/"]

RUN dotnet restore "./HomeLabCore.Api/HomeLabCore.Api.csproj"

COPY . .

WORKDIR "/src/HomeLabCore.Api"
RUN dotnet build "./HomeLabCore.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HomeLabCore.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HomeLabCore.Api.dll"]
