using System;
using System.Net.Http;
using AdControl.Protos;
using AdControl.ScreenClient.Services;
using Avalonia;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdControl.ScreenClient;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Сохраняем провайдер сервисов в App для доступа из окон
        App.Services = host.Services;

        // Запускаем host (необязательно фоновые сервисы, но полезно)
        host.Start();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.AddJsonFile("appsettings.json", true, true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((ctx, services) =>
            {
                var configuration = ctx.Configuration;

                // HttpClient для gateway polling
                services.AddHttpClient("gateway", client =>
                {
                    var baseUrl = configuration["Gateway:Url"] ?? "http://localhost:5000";
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(5);
                });

                // gRPC channel для Avalonia.Logic (HTTP/2 без TLS)
                services.AddSingleton(sp =>
                {
                    var avaloniaUrl = configuration["AvaloniaLogic:GrpcUrl"] ?? "http://localhost:5002";
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return GrpcChannel.ForAddress(avaloniaUrl, new GrpcChannelOptions { HttpHandler = handler });
                });

                services.AddSingleton(sp =>
                    new AvaloniaLogicService.AvaloniaLogicServiceClient(sp.GetRequiredService<GrpcChannel>()));

                // Регистрация PollingClient (код из примера)
                services.AddSingleton<PollingService>();

                // Можно зарегистрировать MainWindow если хочешь: services.AddTransient<MainWindow>();
            });
    }

    // Avalonia configuration
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}