using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string linkedInVersion = "202510.03";

var clientId = Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_SECRET");
var redirectUri = Environment.GetEnvironmentVariable("LINKEDIN_REDIRECT_URI");

if (string.IsNullOrWhiteSpace(clientId) ||
    string.IsNullOrWhiteSpace(clientSecret) ||
    string.IsNullOrWhiteSpace(redirectUri))
{
    throw new InvalidOperationException(
        "Set LINKEDIN_CLIENT_ID, LINKEDIN_CLIENT_SECRET and LINKEDIN_REDIRECT_URI first.");
}

app.MapGet("/", () =>
{
    const string html = """
    <!doctype html>
    <html>
    <head>
        <meta charset="utf-8" />
        <title>LinkedIn Probe</title>
    </head>
    <body style="font-family: sans-serif; max-width: 900px; margin: 40px auto;">
        <h1>LinkedIn Probe</h1>
        <p><a href="/login">Connect LinkedIn</a></p>
        <p>This will request r_profile_basicinfo and then print the raw JSON.</p>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html");
});

app.MapGet("/login", (HttpContext http) =>
{
    var state = Guid.NewGuid().ToString("N");
    http.Response.Cookies.Append(
        "li_oauth_state",
        state,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10)
        });

    var scopes = "r_profile_basicinfo";

    var authUrl =
        "https://www.linkedin.com/oauth/v2/authorization" +
        $"?response_type=code" +
        $"&client_id={Uri.EscapeDataString(clientId)}" +
        $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
        $"&state={Uri.EscapeDataString(state)}" +
        $"&scope={Uri.EscapeDataString(scopes)}";

    return Results.Redirect(authUrl);
});

app.MapGet("/signin-linkedin", async (HttpContext http) =>
{
    var query = http.Request.Query;

    if (query.TryGetValue("error", out var error))
    {
        var description = query["error_description"].ToString();
        return Results.Text($"LinkedIn auth failed.\nerror={error}\ndescription={description}");
    }

    var code = query["code"].ToString();
    var returnedState = query["state"].ToString();
    var expectedState = http.Request.Cookies["li_oauth_state"];

    if (string.IsNullOrWhiteSpace(code))
    {
        return Results.BadRequest("No authorization code returned.");
    }

    if (string.IsNullOrWhiteSpace(expectedState) || !string.Equals(expectedState, returnedState, StringComparison.Ordinal))
    {
        return Results.BadRequest("Invalid OAuth state.");
    }

    using var httpClient = new HttpClient();

    var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["grant_type"] = "authorization_code",
        ["code"] = code,
        ["client_id"] = clientId!,
        ["client_secret"] = clientSecret!,
        ["redirect_uri"] = redirectUri!
    });

    using var tokenResponse = await httpClient.PostAsync(
        "https://www.linkedin.com/oauth/v2/accessToken",
        tokenRequest);

    var tokenJson = await tokenResponse.Content.ReadAsStringAsync();

    Console.WriteLine("==== TOKEN RESPONSE ====");
    Console.WriteLine(PrettyJsonOrRaw(tokenJson));
    Console.WriteLine();

    if (!tokenResponse.IsSuccessStatusCode)
    {
        return Results.Text(
            $"Token exchange failed: {(int)tokenResponse.StatusCode}\n\n{PrettyJsonOrRaw(tokenJson)}",
            "text/plain");
    }

    using var tokenDoc = JsonDocument.Parse(tokenJson);

    if (!tokenDoc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
    {
        return Results.Text("No access_token returned.\n\n" + PrettyJsonOrRaw(tokenJson), "text/plain");
    }

    var accessToken = accessTokenElement.GetString();
    if (string.IsNullOrWhiteSpace(accessToken))
    {
        return Results.Text("access_token was empty.\n\n" + PrettyJsonOrRaw(tokenJson), "text/plain");
    }

    using var profileRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/rest/identityMe");
    profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    profileRequest.Headers.Add("LinkedIn-Version", linkedInVersion);

    using var profileResponse = await httpClient.SendAsync(profileRequest);
    var profileJson = await profileResponse.Content.ReadAsStringAsync();

    Console.WriteLine("==== PROFILE RESPONSE ====");
    Console.WriteLine(PrettyJsonOrRaw(profileJson));
    Console.WriteLine();

    var html = $$"""
    <!doctype html>
    <html>
    <head>
        <meta charset="utf-8" />
        <title>LinkedIn Probe Result</title>
    </head>
    <body style="font-family: sans-serif; max-width: 1100px; margin: 40px auto;">
        <h1>LinkedIn Probe Result</h1>

        <h2>HTTP Status</h2>
        <pre>{{(int)profileResponse.StatusCode}} {{HtmlEncoder.Default.Encode(profileResponse.ReasonPhrase ?? "")}}</pre>

        <h2>Token JSON</h2>
        <pre style="white-space: pre-wrap;">{{HtmlEncoder.Default.Encode(PrettyJsonOrRaw(tokenJson))}}</pre>

        <h2>Profile JSON</h2>
        <pre style="white-space: pre-wrap;">{{HtmlEncoder.Default.Encode(PrettyJsonOrRaw(profileJson))}}</pre>
    </body>
    </html>
    """;

    return Results.Content(html, "text/html");
});

app.Run("http://localhost:5057");

static string PrettyJsonOrRaw(string value)
{
    try
    {
        using var doc = JsonDocument.Parse(value);
        return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
    catch
    {
        return value;
    }
}
