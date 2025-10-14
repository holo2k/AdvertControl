# ========== Build stage ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем только csproj для ускорения restore
COPY Server/AdControl.Protos/AdControl.Protos.csproj Server/AdControl.Protos/
COPY Server/AdControl.Auth/AdControl.Auth.csproj Server/AdControl.Auth/

# Восстанавливаем зависимости
RUN dotnet restore Server/AdControl.Auth/AdControl.Auth.csproj

# Копируем исходники
COPY Server/AdControl.Protos/ Server/AdControl.Protos/
COPY Server/AdControl.Auth/ Server/AdControl.Auth/

# Публикуем проект
RUN dotnet publish Server/AdControl.Auth/AdControl.Auth.csproj -c Release -o /app/publish

# ========== Runtime stage ==========
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5003
ENTRYPOINT ["dotnet", "AdControl.Auth.dll"]

# ========== Development/debug stage ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev
WORKDIR /src
ENV ASPNETCORE_ENVIRONMENT=Development
ENV NUGET_PACKAGES=/root/.nuget/packages

# Копируем только csproj для быстрого restore
COPY Server/AdControl.Protos/AdControl.Protos.csproj Server/AdControl.Protos/
COPY Server/AdControl.Auth/AdControl.Auth.csproj Server/AdControl.Auth/

# Копируем исходники
COPY Server/AdControl.Protos/ Server/AdControl.Protos/
COPY Server/AdControl.Auth/ Server/AdControl.Auth/

# Hot reload для разработки
ENTRYPOINT ["bash", "-lc", "dotnet restore Server/AdControl.Auth/AdControl.Auth.csproj && dotnet watch run --project Server/AdControl.Auth/AdControl.Auth.csproj --urls http://+:5003"]
