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

// Base client — no auth handler; used by AethonApiClient which attaches the token itself
builder.Services.AddHttpClient("AethonApiBase", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Unauthenticated client — used only by the login endpoint before a session exists
builder.Services.AddHttpClient("AethonApiDirect", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Scoped API client — correctly attaches Bearer tokens in both prerender and SignalR phases
builder.Services.AddScoped<AethonApiClient>();

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
    if (loginResult is null)
    {
        return Results.LocalRedirect("/login?error=" + Uri.EscapeDataString("Authentication failed."));
    }

    // 2FA required — redirect to the two-factor verification page
    if (loginResult.RequiresTwoFactor && !string.IsNullOrEmpty(loginResult.TwoFactorTicket))
    {
        return Results.LocalRedirect("/login/two-factor?ticket=" + Uri.EscapeDataString(loginResult.TwoFactorTicket)
            + (!string.IsNullOrWhiteSpace(returnPath) ? "&returnPath=" + Uri.EscapeDataString(returnPath) : ""));
    }

    // Email not yet verified — redirect to check-email page
    if (loginResult.RequiresEmailVerification)
    {
        return Results.LocalRedirect("/register/check-email?email=" + Uri.EscapeDataString(loginResult.Email)
            + "&unverified=1");
    }

    if (string.IsNullOrEmpty(loginResult.Token))
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
    claimsList.Add(new Claim(AppClaimTypes.IsSuperAdmin, loginResult.IsSuperAdmin ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.IsAdmin, loginResult.IsAdmin ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.IsSupport, loginResult.IsSupport ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.MustChangePassword, loginResult.MustChangePassword ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.MustEnableMfa, loginResult.MustEnableMfa ? "true" : "false"));

    var claims = claimsList.ToArray();

    var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(IdentityConstants.ApplicationScheme, principal);

    // Forced actions take priority over the normal redirect destination
    if (loginResult.MustChangePassword)
        return Results.LocalRedirect("/account/change-password");
    if (loginResult.MustEnableMfa)
        return Results.LocalRedirect("/account/setup-mfa");

    var isStaff = loginResult.IsSuperAdmin || loginResult.IsAdmin || loginResult.IsSupport;
    var redirect = !string.IsNullOrWhiteSpace(returnPath) && returnPath.StartsWith('/')
        ? returnPath
        : isStaff ? "/admin" : "/";

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

// Two-factor verification — completes the 2FA login flow
app.MapPost("/account/verify-2fa", async (
    HttpContext ctx,
    IHttpClientFactory factory,
    [FromForm] string ticket,
    [FromForm] string code,
    [FromForm] string? returnPath) =>
{
    var client = factory.CreateClient("AethonApiDirect");

    HttpResponseMessage apiResponse;
    try
    {
        apiResponse = await client.PostAsJsonAsync("/api/v1/auth/verify-2fa", new { twoFactorTicket = ticket, code });
    }
    catch
    {
        return Results.LocalRedirect("/login/two-factor?ticket=" + Uri.EscapeDataString(ticket)
            + "&error=" + Uri.EscapeDataString("Unable to reach authentication service."));
    }

    if (!apiResponse.IsSuccessStatusCode)
    {
        return Results.LocalRedirect("/login/two-factor?ticket=" + Uri.EscapeDataString(ticket)
            + "&error=" + Uri.EscapeDataString("Invalid or expired verification code.")
            + (!string.IsNullOrWhiteSpace(returnPath) ? "&returnPath=" + Uri.EscapeDataString(returnPath) : ""));
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
    claimsList.Add(new Claim(AppClaimTypes.IsSuperAdmin, loginResult.IsSuperAdmin ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.IsAdmin, loginResult.IsAdmin ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.IsSupport, loginResult.IsSupport ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.MustChangePassword, loginResult.MustChangePassword ? "true" : "false"));
    claimsList.Add(new Claim(AppClaimTypes.MustEnableMfa, loginResult.MustEnableMfa ? "true" : "false"));

    var identity = new ClaimsIdentity(claimsList.ToArray(), IdentityConstants.ApplicationScheme);
    var principal = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(IdentityConstants.ApplicationScheme, principal);

    if (loginResult.MustChangePassword)
        return Results.LocalRedirect("/account/change-password");
    if (loginResult.MustEnableMfa)
        return Results.LocalRedirect("/account/setup-mfa");

    var isStaff = loginResult.IsSuperAdmin || loginResult.IsAdmin || loginResult.IsSupport;
    var redirect = !string.IsNullOrWhiteSpace(returnPath) && returnPath.StartsWith('/')
        ? returnPath
        : isStaff ? "/admin" : "/";

    return Results.LocalRedirect(redirect);
}).DisableAntiforgery();

// ── Sitemap shared helpers ────────────────────────────────────────────────────
static string SitemapXmlEsc(string s) => s
    .Replace("&", "&amp;")
    .Replace("<", "&lt;")
    .Replace(">", "&gt;")
    .Replace("\"", "&quot;")
    .Replace("'", "&apos;");

static string SitemapJobSlug(string title, Guid id)
{
    var clean = System.Text.RegularExpressions.Regex.Replace(title.ToLowerInvariant(), @"[^a-z0-9\s_]", "");
    var slug  = System.Text.RegularExpressions.Regex.Replace(clean.Trim(), @"\s+", "-");
    return $"{slug}-{id}";
}

static string GetSiteBase(HttpContext ctx, IConfiguration cfg)
    => (cfg["SiteBaseUrl"] ?? $"{ctx.Request.Scheme}://{ctx.Request.Host}").TrimEnd('/');

// GET /sitemap.xml — sitemap index referencing all child sitemaps.
// The number of jobs-N.xml entries is determined by live record counts from the API.
app.MapGet("/sitemap.xml", async (HttpContext ctx, IHttpClientFactory factory, IConfiguration config, CancellationToken ct) =>
{
    var siteBase = GetSiteBase(ctx, config);
    var today    = DateTime.UtcNow.ToString("yyyy-MM-dd");
    var client   = factory.CreateClient("AethonApiDirect");

    var jobTotalPages = 1;
    try
    {
        var resp = await client.GetAsync("/api/v1/public/sitemap/stats", ct);
        if (resp.IsSuccessStatusCode)
        {
            var stats = await resp.Content.ReadFromJsonAsync<SitemapStats>(cancellationToken: ct);
            if (stats is not null) jobTotalPages = Math.Max(1, stats.JobTotalPages);
        }
    }
    catch { /* API unreachable — emit index with 1 job page */ }

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

    void AddEntry(string loc)
    {
        sb.AppendLine("  <sitemap>");
        sb.AppendLine($"    <loc>{SitemapXmlEsc(loc)}</loc>");
        sb.AppendLine($"    <lastmod>{today}</lastmod>");
        sb.AppendLine("  </sitemap>");
    }

    AddEntry($"{siteBase}/sitemaps/static.xml");
    AddEntry($"{siteBase}/sitemaps/organisations.xml");
    for (var i = 1; i <= jobTotalPages; i++)
        AddEntry($"{siteBase}/sitemaps/jobs-{i}.xml");

    sb.Append("</sitemapindex>");
    return Results.Content(sb.ToString(), "application/xml; charset=utf-8");
});

// GET /sitemaps/static.xml — home page, job search page, and all /info/* pages.
app.MapGet("/sitemaps/static.xml", (HttpContext ctx, IConfiguration config) =>
{
    var siteBase = GetSiteBase(ctx, config);
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

    void AddUrl(string loc, string changefreq, string priority)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{SitemapXmlEsc(loc)}</loc>");
        sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        sb.AppendLine($"    <priority>{priority}</priority>");
        sb.AppendLine("  </url>");
    }

    AddUrl($"{siteBase}/",                                "daily",   "1.0");
    AddUrl($"{siteBase}/jobs",                            "hourly",  "0.9");
    AddUrl($"{siteBase}/info/equal-opportunity-employer", "monthly", "0.5");
    AddUrl($"{siteBase}/info/accessible-workplace",       "monthly", "0.5");
    AddUrl($"{siteBase}/info/enhanced-verification",      "monthly", "0.5");
    AddUrl($"{siteBase}/info/standard-verification",      "monthly", "0.5");
    AddUrl($"{siteBase}/info/privacy",                    "monthly", "0.4");

    sb.Append("</urlset>");
    return Results.Content(sb.ToString(), "application/xml; charset=utf-8");
});

// GET /sitemaps/organisations.xml — all public organisation profile pages, enhanced-verified first.
app.MapGet("/sitemaps/organisations.xml", async (HttpContext ctx, IHttpClientFactory factory, IConfiguration config, CancellationToken ct) =>
{
    var siteBase = GetSiteBase(ctx, config);
    var client   = factory.CreateClient("AethonApiDirect");

    List<SitemapOrgItem> orgs = [];
    try
    {
        var resp = await client.GetAsync("/api/v1/public/sitemap/orgs", ct);
        if (resp.IsSuccessStatusCode)
            orgs = await resp.Content.ReadFromJsonAsync<List<SitemapOrgItem>>(cancellationToken: ct) ?? [];
    }
    catch { /* API unreachable — emit empty sitemap */ }

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

    foreach (var org in orgs)
    {
        if (string.IsNullOrWhiteSpace(org.Slug)) continue;
        var (freq, pri) = org.Tier switch
        {
            2 => ("weekly",  "0.9"),
            1 => ("weekly",  "0.7"),
            _ => ("monthly", "0.5")
        };
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{SitemapXmlEsc($"{siteBase}/organisations/{org.Slug}")}</loc>");
        if (org.LastMod is not null) sb.AppendLine($"    <lastmod>{org.LastMod}</lastmod>");
        sb.AppendLine($"    <changefreq>{freq}</changefreq>");
        sb.AppendLine($"    <priority>{pri}</priority>");
        sb.AppendLine("  </url>");
    }

    sb.Append("</urlset>");
    return Results.Content(sb.ToString(), "application/xml; charset=utf-8");
});

// GET /sitemaps/jobs-{page}.xml — paginated job pages, 50,000 URLs per file.
// Enhanced-verified employer jobs appear first.
app.MapGet("/sitemaps/jobs-{page}.xml", async (HttpContext ctx, IHttpClientFactory factory, IConfiguration config, string page, CancellationToken ct) =>
{
    if (!int.TryParse(page, out var pageNum) || pageNum < 1)
        return Results.NotFound();

    var siteBase = GetSiteBase(ctx, config);
    var client   = factory.CreateClient("AethonApiDirect");

    SitemapJobsPage? jobPage = null;
    try
    {
        var resp = await client.GetAsync($"/api/v1/public/sitemap/jobs?page={pageNum}", ct);
        if (resp.IsSuccessStatusCode)
            jobPage = await resp.Content.ReadFromJsonAsync<SitemapJobsPage>(cancellationToken: ct);
    }
    catch { /* API unreachable — emit empty sitemap */ }

    if (jobPage is not null && pageNum > jobPage.TotalPages)
        return Results.NotFound();

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
    sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

    if (jobPage is not null)
    {
        foreach (var job in jobPage.Items)
        {
            if (string.IsNullOrWhiteSpace(job.OrgSlug) || string.IsNullOrWhiteSpace(job.Title)) continue;
            var slug = SitemapJobSlug(job.Title, job.Id);
            var (freq, pri) = job.OrgTier switch
            {
                2 => ("daily",  "0.8"),
                1 => ("daily",  "0.6"),
                _ => ("weekly", "0.4")
            };
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{SitemapXmlEsc($"{siteBase}/jobs/{job.OrgSlug}/{slug}")}</loc>");
            if (job.LastMod is not null) sb.AppendLine($"    <lastmod>{job.LastMod}</lastmod>");
            sb.AppendLine($"    <changefreq>{freq}</changefreq>");
            sb.AppendLine($"    <priority>{pri}</priority>");
            sb.AppendLine("  </url>");
        }
    }

    sb.Append("</urlset>");
    return Results.Content(sb.ToString(), "application/xml; charset=utf-8");
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal sealed class SitemapStats
{
    public int OrgCount      { get; set; }
    public int JobCount      { get; set; }
    public int JobPageSize   { get; set; }
    public int JobTotalPages { get; set; }
}

internal sealed class SitemapOrgItem
{
    public string? Slug    { get; set; }
    public int     Tier    { get; set; }
    public string? LastMod { get; set; }
}

internal sealed class SitemapJobItem
{
    public Guid    Id      { get; set; }
    public string? Title   { get; set; }
    public string? OrgSlug { get; set; }
    public int     OrgTier { get; set; }
    public string? LastMod { get; set; }
}

internal sealed class SitemapJobsPage
{
    public int                   Page       { get; set; }
    public int                   TotalPages { get; set; }
    public int                   TotalCount { get; set; }
    public List<SitemapJobItem>  Items      { get; set; } = [];
}

internal sealed class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AppType { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsSupport { get; set; }
    public string? OrganisationId { get; set; }
    public string? OrganisationName { get; set; }
    public string? OrganisationType { get; set; }
    public string? CompanyRole { get; set; }
    public string? RecruiterRole { get; set; }
    public bool IsOrganisationOwner { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public bool RequiresEmailVerification { get; set; }
    public string? TwoFactorTicket { get; set; }
    public bool MustChangePassword { get; set; }
    public bool MustEnableMfa { get; set; }
}
