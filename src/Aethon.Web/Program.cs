using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Aethon.Shared.Auth;
using Aethon.Web.Components;
using Aethon.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5201";

// Authenticated client — adds Bearer token from signed-in user
builder.Services.AddHttpClient("AethonApi", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    })
    .AddHttpMessageHandler<ApiAuthCookieHandler>();

// Unauthenticated client — used only by the login endpoint before a session exists
builder.Services.AddHttpClient("AethonApiDirect", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

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

// Login form handler — must be before UseAntiforgery
app.MapPost("/account/login", async (
    HttpContext ctx,
    IHttpClientFactory factory,
    [FromForm] string email,
    [FromForm] string password,
    [FromForm] string? returnPath) =>
{
    var client = factory.CreateClient("AethonApiDirect");

    HttpResponseMessage apiResponse;
    try
    {
        apiResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
    }
    catch
    {
        return Results.LocalRedirect("/login?error=" + Uri.EscapeDataString("Unable to reach authentication service."));
    }

    if (!apiResponse.IsSuccessStatusCode)
    {
        return Results.LocalRedirect("/login?error=" + Uri.EscapeDataString("Invalid email or password."));
    }

    var loginResult = await apiResponse.Content.ReadFromJsonAsync<LoginResult>();
    if (loginResult is null || string.IsNullOrEmpty(loginResult.Token))
    {
        return Results.LocalRedirect("/login?error=" + Uri.EscapeDataString("Authentication failed."));
    }

    var claimsList = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, loginResult.UserId),
        new Claim(ClaimTypes.Email, loginResult.Email),
        new Claim(ClaimTypes.Name, loginResult.DisplayName),
        new Claim("access_token", loginResult.Token),
        new Claim(AppClaimTypes.DisplayName, loginResult.DisplayName),
        new Claim(AppClaimTypes.AppType, loginResult.AppType ?? string.Empty)
    };

    if (!string.IsNullOrEmpty(loginResult.OrganisationId))
        claimsList.Add(new Claim(AppClaimTypes.OrganisationId, loginResult.OrganisationId));
    if (!string.IsNullOrEmpty(loginResult.OrganisationName))
        claimsList.Add(new Claim(AppClaimTypes.OrganisationName, loginResult.OrganisationName));
    if (!string.IsNullOrEmpty(loginResult.OrganisationType))
        claimsList.Add(new Claim(AppClaimTypes.OrganisationType, loginResult.OrganisationType));
    if (!string.IsNullOrEmpty(loginResult.CompanyRole))
        claimsList.Add(new Claim(AppClaimTypes.CompanyRole, loginResult.CompanyRole));
    if (!string.IsNullOrEmpty(loginResult.RecruiterRole))
        claimsList.Add(new Claim(AppClaimTypes.RecruiterRole, loginResult.RecruiterRole));
    claimsList.Add(new Claim(AppClaimTypes.IsOrganisationOwner, loginResult.IsOrganisationOwner ? "true" : "false"));

    var claims = claimsList.ToArray();

    var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(IdentityConstants.ApplicationScheme, principal);

    var redirect = !string.IsNullOrWhiteSpace(returnPath) && returnPath.StartsWith('/')
        ? returnPath
        : "/";

    return Results.LocalRedirect(redirect);
}).DisableAntiforgery();

app.MapPost("/account/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(IdentityConstants.ApplicationScheme);
    return Results.LocalRedirect("/login");
}).DisableAntiforgery();

// GET signout — used by server-side Blazor when claims must be refreshed after a state change
// (e.g. invite acceptance). Signs out and redirects to login with an optional message.
app.MapGet("/account/signout", async (HttpContext ctx, string? reason) =>
{
    await ctx.SignOutAsync(IdentityConstants.ApplicationScheme);
    var redirect = string.IsNullOrWhiteSpace(reason)
        ? "/login"
        : $"/login?message={Uri.EscapeDataString(reason)}";
    return Results.LocalRedirect(redirect);
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal sealed class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AppType { get; set; } = string.Empty;
    public string? OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
    public string? OrganisationType { get; set; }
    public string? CompanyRole { get; set; }
    public string? RecruiterRole { get; set; }
    public bool IsOrganisationOwner { get; set; }
}
