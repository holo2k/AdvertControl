using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AdControl.Gateway.Application.Minio;
using AdControl.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

ConfigureKestrel(builder);
ConfigureServices(builder);

var app = builder.Build();

ConfigureMiddleware(app);

app.MapControllers();
app.MapGet("/", () => Results.Ok("AdControl.Gateway running"));

app.Run();

static void ConfigureKestrel(WebApplicationBuilder builder)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        var port = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"), out var p) ? p : 5000;
        options.ListenAnyIP(port, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
    });
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    // Controllers + JSON
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

    // Swagger
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
                Array.Empty<string>()
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
    });

    // Redis
    var redisUrl = builder.Configuration.GetValue<string>("Redis:Url") ?? "localhost:6379";
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisUrl));

    // Keycloak JWT auth
    var keycloakAuthority = builder.Configuration.GetValue<string>("Keycloak:Authority");
    if (!string.IsNullOrEmpty(keycloakAuthority))
        ConfigureAuthentication(builder, keycloakAuthority);

    // gRPC clients
    ConfigureGrpcClients(builder);

    // MinIO
    ConfigureMinio(builder);

    // CORS
    builder.Services.AddCors(p =>
        p.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
}

static void ConfigureAuthentication(WebApplicationBuilder builder, string authority)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.RequireHttpsMetadata = false; // dev only
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "preferred_username",
                RoleClaimType = ClaimTypes.Role,
                ValidateAudience = false,
                ValidateIssuer = true
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = ctx =>
                {
                    if (ctx.SecurityToken is JsonWebToken jsonJwt)
                    {
                        var payloadJson = Base64UrlDecode(jsonJwt.EncodedPayload);
                        using var doc = JsonDocument.Parse(payloadJson);
                        if (doc.RootElement.TryGetProperty("realm_access", out var realmAccess) &&
                            realmAccess.TryGetProperty("roles", out var rolesEl))
                        {
                            var identity = ctx.Principal!.Identity as ClaimsIdentity;
                            foreach (var r in rolesEl.EnumerateArray())
                            {
                                var role = r.GetString();
                                if (!string.IsNullOrEmpty(role))
                                    identity!.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();
}

static string Base64UrlDecode(string input)
{
    var padded = input.Replace('-', '+').Replace('_', '/');
    switch (padded.Length % 4)
    {
        case 2:
            padded += "==";
            break;
        case 3:
            padded += "=";
            break;
    }

    var bytes = Convert.FromBase64String(padded);
    return Encoding.UTF8.GetString(bytes);
}

static void ConfigureGrpcClients(WebApplicationBuilder builder)
{
    AddGrpcClient<ScreenService.ScreenServiceClient>(builder, "Grpc:ScreenService", "http://localhost:5001");
    AddGrpcClient<AvaloniaLogicService.AvaloniaLogicServiceClient>(builder, "Grpc:AvaloniaLogicService",
        "http://localhost:5002");
    AddGrpcClient<AuthService.AuthServiceClient>(builder, "Grpc:AuthService", "http://localhost:5003");
    AddGrpcClient<FileService.FileServiceClient>(builder, "Grpc:FileService", "https://adcontrol-web:5001");
}

static void AddGrpcClient<TClient>(WebApplicationBuilder builder, string configKey, string defaultAddress)
    where TClient : class
{
    builder.Services.AddGrpcClient<TClient>(o =>
            o.Address = new Uri(builder.Configuration[configKey] ?? defaultAddress))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
}

static void ConfigureMinio(WebApplicationBuilder builder)
{
    var cfg = builder.Configuration.GetSection("Minio");
    var endpoint = cfg.GetValue<string>("Endpoint");
    if (string.IsNullOrEmpty(endpoint)) return;

    var access = cfg.GetValue<string>("AccessKey");
    var secret = cfg.GetValue<string>("SecretKey");
    var secure = cfg.GetValue<bool?>("Secure") ?? false;

    var client = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(access, secret);

    if (!secure) client = client.WithSSL(false);

    builder.Services.AddSingleton(client.Build());
    builder.Services.Configure<MinioSettings>(cfg);
}

static void ConfigureMiddleware(WebApplication app)
{
    app.UseCors();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    var keycloakAuthority = app.Configuration.GetValue<string>("Keycloak:Authority");
    if (!string.IsNullOrEmpty(keycloakAuthority))
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}