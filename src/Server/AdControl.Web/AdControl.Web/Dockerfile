# ========== Build stage ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем только csproj для кэша зависимостей
COPY Server/AdControl.Core/AdControl.Core.csproj Server/AdControl.Core/
COPY Server/AdControl.Protos/AdControl.Protos.csproj Server/AdControl.Protos/
COPY Server/AdControl.Web/AdControl.Web/AdControl.Web.csproj Server/AdControl.Web/AdControl.Web/
COPY Server/AdControl.Web/AdControl.Domain/AdControl.Domain.csproj Server/AdControl.Web/AdControl.Domain/
COPY Server/AdControl.Web/AdControl.Application/AdControl.Application.csproj Server/AdControl.Web/AdControl.Application/
COPY Server/AdControl.Gateway/AdControl.Gateway.Application/AdControl.Gateway.Application.csproj Server/AdControl.Gateway/AdControl.Gateway.Application/
# Восстанавливаем зависимости
RUN dotnet restore "Server/AdControl.Web/AdControl.Web/AdControl.Web.csproj"

# Копируем весь код
COPY Server/AdControl.Core/ Server/AdControl.Core/
COPY Server/AdControl.Protos/ Server/AdControl.Protos/
COPY Server/AdControl.Web/AdControl.Web/ Server/AdControl.Web/AdControl.Web/
COPY Server/AdControl.Web/AdControl.Domain/ Server/AdControl.Web/AdControl.Domain/
COPY Server/AdControl.Web/AdControl.Application/ Server/AdControl.Web/AdControl.Application/
COPY Server/AdControl.Gateway/AdControl.Gateway.Application/ Server/AdControl.Gateway/AdControl.Gateway.Application/

# Сборка и публикация
RUN dotnet publish "Server/AdControl.Web/AdControl.Web/AdControl.Web.csproj" -c Release -o /app/publish

# ========== Runtime stage ==========
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "AdControl.Web.dll"]

# ========== Development/debug stage ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev
WORKDIR /src
ENV ASPNETCORE_ENVIRONMENT=Development
ENV NUGET_PACKAGES=/root/.nuget/packages

# Копируем все проекты для корректной структуры при дебаге
COPY Server/AdControl.Core/ Server/AdControl.Core/
COPY Server/AdControl.Protos/ Server/AdControl.Protos/
COPY Server/AdControl.Web/AdControl.Web/ Server/AdControl.Web/AdControl.Web/
COPY Server/AdControl.Web/AdControl.Domain/ Server/AdControl.Web/AdControl.Domain/
COPY Server/AdControl.Web/AdControl.Application/ Server/AdControl.Web/AdControl.Application/
COPY Server/AdControl.Gateway/AdControl.Gateway.Application/ Server/AdControl.Gateway/AdControl.Gateway.Application/

# Hot reload для дебага
ENTRYPOINT ["bash", "-lc", "dotnet restore Server/AdControl.Web/AdControl.Web/AdControl.Web.csproj && dotnet watch run --project Server/AdControl.Web/AdControl.Web/AdControl.Web.csproj --urls http://+:5001"]
