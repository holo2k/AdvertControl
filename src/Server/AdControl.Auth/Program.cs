using AdControl.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

ConfigureKestrel(builder);
ConfigureAuthentication(builder);
ConfigureServices(builder);

var app = builder.Build();

await InitializeKeycloakAsync(app);

ConfigureMiddleware(app);
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

static void ConfigureAuthentication(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOpenIdConnect(options =>
        {
            options.Authority = "http://keycloak:8080/realms/myrealm";
            options.ClientId = "admin-cli";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        });

    builder.Services.AddAuthorization();
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddOpenApi();
    builder.Services.Configure<KeycloakOptions>(opt =>
    {
        opt.AdminUser = Environment.GetEnvironmentVariable("KEYCLOAK_DEFAULT_ADMIN");
        opt.AdminPassword = Environment.GetEnvironmentVariable("KEYCLOAK_DEFAULT_PASSWORD");
        opt.AdminClientId = "admin-cli";
        opt.AdminClientSecret = "secret";
    });
    builder.Services.AddHttpClient<IKeycloakSetupService, KeycloakSetupService>();
    builder.Services.AddGrpc();
}

static async Task InitializeKeycloakAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var keycloakService = scope.ServiceProvider.GetRequiredService<IKeycloakSetupService>();
    await keycloakService.EnsureSetupAsync();
}

static void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment()) app.MapOpenApi();

    app.MapGet("/", () => "Auth is running");
}

static void ConfigureGrpc(WebApplication app)
{
    app.MapGrpcService<AuthService>();
}