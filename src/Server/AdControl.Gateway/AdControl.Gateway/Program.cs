using AdControl.Gateway.Application.Minio;
using AdControl.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Kestrel: ������� ������ ���� � ��������� HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    var port = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"), out var p) ? p : 5000;
    options.ListenAnyIP(port, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:Url") ?? "localhost:6379"));

// Keycloak auth
var keycloakAuthority = builder.Configuration.GetValue<string>("Keycloak:Authority");
if (!string.IsNullOrEmpty(keycloakAuthority))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = keycloakAuthority;
            options.Audience = builder.Configuration.GetValue<string>("Keycloak:Audience") ?? "gateway";
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = true };
        });
    builder.Services.AddAuthorization();
}

// gRPC clients
builder.Services
    .AddGrpcClient<ScreenService.ScreenServiceClient>(o =>
        o.Address = new Uri(builder.Configuration["Grpc:ScreenService"] ?? "http://localhost:5001"))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });


builder.Services.AddGrpcClient<AvaloniaLogicService.AvaloniaLogicServiceClient>(o =>
        o.Address = new Uri(builder.Configuration["Grpc:AvaloniaLogicService"] ?? "http://localhost:5002"))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services
    .AddGrpcClient<AuthService.AuthServiceClient>(o =>
        o.Address = new Uri(builder.Configuration["Grpc:AuthService"] ?? "http://localhost:5003"))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// MinIO client
var minioCfg = builder.Configuration.GetSection("Minio");
var minioEndpoint = minioCfg.GetValue<string>("Endpoint");
var minioAccess = minioCfg.GetValue<string>("AccessKey");
var minioSecret = minioCfg.GetValue<string>("SecretKey");
var minioSecure = minioCfg.GetValue<bool?>("Secure") ?? false;

if (!string.IsNullOrEmpty(minioEndpoint))
{
    var minioClient = new MinioClient()
        .WithEndpoint(minioEndpoint)
        .WithCredentials(minioAccess, minioSecret);

    if (!minioSecure) minioClient = minioClient.WithSSL(false);
    builder.Services.AddSingleton(minioClient.Build());
    builder.Services.Configure<MinioSettings>(minioCfg);
}

builder.Services.AddCors(p => p.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!string.IsNullOrEmpty(keycloakAuthority))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();
app.MapGet("/", () => Results.Ok("AdControl.Gateway running"));
app.Run();