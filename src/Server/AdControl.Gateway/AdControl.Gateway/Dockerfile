# ========== Build stage ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем только csproj-файлы для restore
COPY Server/AdControl.Protos/AdControl.Protos.csproj Server/AdControl.Protos/
COPY Server/AdControl.Gateway/AdControl.Gateway/AdControl.Gateway.csproj Server/AdControl.Gateway/AdControl.Gateway/
COPY Server/AdControl.Gateway/AdControl.Gateway.Application/AdControl.Gateway.Application.csproj Server/AdControl.Gateway/AdControl.Gateway.Application/

# Восстанавливаем зависимости отдельно, чтобы кешировать NuGet
RUN dotnet restore "Server/AdControl.Gateway/AdControl.Gateway/AdControl.Gateway.csproj"

# Теперь копируем всё остальное с сохранением структуры
COPY Server/AdControl.Protos/ Server/AdControl.Protos/
COPY Server/AdControl.Gateway/AdControl.Gateway/ Server/AdControl.Gateway/AdControl.Gateway/
COPY Server/AdControl.Gateway/AdControl.Gateway.Application/ Server/AdControl.Gateway/AdControl.Gateway.Application/

# Собираем и публикуем
RUN dotnet publish "Server/AdControl.Gateway/AdControl.Gateway/AdControl.Gateway.csproj" -c Release -o /app/publish

# ========== Runtime stage ==========
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "AdControl.Gateway.dll"]

# ========== Dev/debug stage ==========
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev
WORKDIR /src
ENV ASPNETCORE_ENVIRONMENT=Development

# Копируем структуру полностью
COPY Server/AdControl.Protos/ Server/AdControl.Protos/
COPY Server/AdControl.Gateway/AdControl.Gateway/ Server/AdControl.Gateway/AdControl.Gateway/
COPY Server/AdControl.Gateway/AdControl.Gateway.Application/ Server/AdControl.Gateway/AdControl.Gateway.Application/

ENTRYPOINT ["bash", "-lc", "dotnet restore Server/AdControl.Gateway/AdControl.Gateway/AdControl.Gateway.csproj && dotnet run --project Server/AdControl.Gateway/AdControl.Gateway/AdControl.Gateway.csproj --urls http://+:80"]
