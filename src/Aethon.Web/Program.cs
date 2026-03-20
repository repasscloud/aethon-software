using System.Security.Cryptography.X509Certificates;
using Aethon.Web.Components;
using Aethon.Web.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var dpBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeysPath"] ?? "/keys"))
    .SetApplicationName(builder.Configuration["DataProtection:ApplicationName"] ?? "Aethon");

var certBase64 = builder.Configuration["DataProtection:CertBase64"];
if (!string.IsNullOrEmpty(certBase64))
{
    var certBytes = Convert.FromBase64String(certBase64);
    var cert = X509CertificateLoader.LoadPkcs12(certBytes, password: null);
    dpBuilder.ProtectKeysWithCertificate(cert);
}

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = "Aethon.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = "/";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazorBootstrap();

builder.Services.AddTransient<ApiAuthCookieHandler>();

builder.Services.AddHttpClient("AethonApi", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5201");
    })
    .AddHttpMessageHandler<ApiAuthCookieHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

if (builder.Configuration.GetValue("EnableHttpsRedirection", app.Environment.IsDevelopment()))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();