FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GestionEmps.API/GestionEmps.API.csproj", "GestionEmps.API/"]
COPY ["GestionEmps.Application/GestionEmps.Application.csproj", "GestionEmps.Application/"]
COPY ["GestionEmps.Core/GestionEmps.Core.csproj", "GestionEmps.Core/"]
COPY ["GestionEmps.Infrastructure/GestionEmps.Infrastructure.csproj", "GestionEmps.Infrastructure/"]
RUN dotnet restore "GestionEmps.API/GestionEmps.API.csproj"
COPY . .
WORKDIR "/src/GestionEmps.API"
RUN dotnet build "./GestionEmps.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GestionEmps.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GestionEmps.API.dll"]
