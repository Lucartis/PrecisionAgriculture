﻿FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DataHub/PrecisionAgriculture.DataHub.csproj", "src/DataHub/"]
RUN dotnet restore "src/DataHub/PrecisionAgriculture.DataHub.csproj"
COPY . .
WORKDIR "/src/src/DataHub"
RUN dotnet build "./PrecisionAgriculture.DataHub.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PrecisionAgriculture.DataHub.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PrecisionAgriculture.DataHub.dll"]
