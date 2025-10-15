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

// Kestrel: слушаем нужный порт и разрешаем HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    var port = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"), out var p) ? p : 5002;
    var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
    var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

    options.ListenAnyIP(port, listenOptions =>
    {
        listenOptions.UseHttps(certPath, certPassword);
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// configuration
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? "Host=postgres;Port=5432;Database=adcontrol;Username=aduser;Password=secret";

// EF DbContext
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

// register application services
builder.Services.AddScoped<IScreenRepository, ScreenRepository>();
builder.Services.AddScoped<IConfigRepository, ConfigRepository>();
builder.Services.AddScoped<IScreenService, ScreenService>();
builder.Services.AddScoped<IConfigService, ConfigService>();

// gRPC
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GrpcAvaloniaService>();
app.MapGet("/", () => "AdControl.Avalonia.Logic gRPC running");

app.Run();