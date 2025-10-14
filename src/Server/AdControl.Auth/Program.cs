using AdControl.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

builder.WebHost.ConfigureKestrel(options =>
{
    var port = int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"), out var p) ? p : 5003;
    var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
    var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

    options.ListenAnyIP(port, listenOptions =>
    {
        listenOptions.UseHttps(certPath, certPassword);
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var keycloakService = scope.ServiceProvider.GetRequiredService<IKeycloakSetupService>();
    await keycloakService.EnsureSetupAsync();
}

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapGrpcService<AuthService>();


app.MapGet("/", () => "Auth is running");

app.Run();