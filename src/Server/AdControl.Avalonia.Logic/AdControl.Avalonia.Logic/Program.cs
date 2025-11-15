using AdControl.Application.Repository.Abstractions;
using AdControl.Application.Services.Abstractions;
using AdControl.Application.Services.Implementations;
using AdControl.Avalonia.Logic.Services;
using AdControl.Core.Infrastructure.Repository.Implementations;
using AdControl.Core.Persistence;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

ConfigureKestrel(builder);
ConfigureServices(builder);

var app = builder.Build();

ConfigureGrpc(app);

app.Run();

static void ConfigureKestrel(WebApplicationBuilder builder)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        var port = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"), out var p) ? p : 5001;
        var certPath = Environment.GetEnvironmentVariable("CERT_PATH") ?? "/app/certs/cert.pfx";
        var certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD") ?? "YourPwd";
        options.ListenAnyIP(port, listenOptions =>
        {
            listenOptions.UseHttps(certPath, certPassword);
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    });
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? "Host=postgres;Port=5432;Database=adcontrol;Username=aduser;Password=secret";

    // EF DbContext
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

    // Application services
    builder.Services.AddScoped<IScreenRepository, ScreenRepository>();
    builder.Services.AddScoped<IConfigRepository, ConfigRepository>();
    builder.Services.AddScoped<IScreenService, ScreenService>();
    builder.Services.AddScoped<IConfigService, ConfigService>();

    // gRPC
    builder.Services.AddGrpc();
}

static void ConfigureGrpc(WebApplication app)
{
    app.MapGrpcService<GrpcAvaloniaService>();
    app.MapGet("/", () => "AdControl.Avalonia.Logic gRPC running");
}