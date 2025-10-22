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

// Kestrel: слушаем нужный порт и разрешаем HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    var port = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"), out var p) ? p : 5001;
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
builder.Services.Configure<MinioSettings>(
    builder.Configuration.GetSection("Minio"));
builder.Services.AddSingleton<MinioFileService>(sp =>
    new MinioFileService(sp.GetRequiredService<IOptions<MinioSettings>>()));


var app = builder.Build();

// Ensure DB created (dev convenience). Use migrations in prod.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapGrpcService<GrpcScreenService>();
app.MapGrpcService<GrpcMinioService>();

app.MapGet("/", () => "AdControl.Web gRPC running");

app.Run();