using AdControl.Application.Repository.Abstractions;
using AdControl.Application.Services.Abstractions;
using AdControl.Application.Services.Implementations;
using AdControl.Core.Infrastructure.Repository.Implementations;
using AdControl.Core.Persistence;
using AdControl.Gateway.Application.Minio;
using AdControl.Web.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

ConfigureKestrel(builder);
ConfigureServices(builder);

var app = builder.Build();

EnsureDatabaseCreated(app);

MapEndpoints(app);

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
    // Configuration
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

    // Minio
    builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));
    builder.Services.AddSingleton<MinioFileService>(sp =>
        new MinioFileService(sp.GetRequiredService<IOptions<MinioSettings>>()));
}

static void EnsureDatabaseCreated(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

static void MapEndpoints(WebApplication app)
{
    app.MapGrpcService<GrpcScreenService>();
    app.MapGrpcService<GrpcMinioService>();
    app.MapGet("/", () => "AdControl.Web gRPC running");
}