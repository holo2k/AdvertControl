using System.Text;
using System.Text.Json;
using AdControl.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

ConfigureKestrel(builder);
ConfigureAuthentication(builder);
ConfigureServices(builder);

var app = builder.Build();

await InitializeKeycloakAsync(app);
await ConfigureRealmTokenSettingsAsync(app);

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

static async Task ConfigureRealmTokenSettingsAsync(WebApplication app)
{
    var http = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
    var opts = app.Services.GetRequiredService<IOptions<KeycloakOptions>>().Value;

    var realm = "myrealm";
    var host = "http://keycloak:8080";

    // 1. Получаем admin token
    var tokenReq = new HttpRequestMessage(HttpMethod.Post, $"{host}/realms/master/protocol/openid-connect/token");
    tokenReq.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["client_id"] = opts.AdminClientId,
        ["client_secret"] = opts.AdminClientSecret,
        ["username"] = opts.AdminUser,
        ["password"] = "admin",
        ["grant_type"] = "password"
    });

    var tokenResp = await http.SendAsync(tokenReq);
    tokenResp.EnsureSuccessStatusCode();
    var tokenJson = JsonDocument.Parse(await tokenResp.Content.ReadAsStringAsync());
    var adminToken = tokenJson.RootElement.GetProperty("access_token").GetString();

    // 2. Формируем запрос на обновление tokenSettings
    var patchReq = new HttpRequestMessage(HttpMethod.Put, $"{host}/admin/realms/{realm}");
    patchReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

    var body = new
    {
        tokenSettings = new
        {
            accessTokenLifespan = "999999999",
            accessTokenLifespanForImplicitFlow = "999999999",
            ssoSessionIdleTimeout = "999999999",
            ssoSessionMaxLifespan = "999999999",
            clientSessionIdleTimeout = "999999999",
            clientSessionMaxLifespan = "999999999"
        }
    };

    patchReq.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    var patchResp = await http.SendAsync(patchReq);
    patchResp.EnsureSuccessStatusCode();
}