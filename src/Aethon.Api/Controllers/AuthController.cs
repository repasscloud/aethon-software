using System.Security.Claims;
using Aethon.Api.Auth;
using Aethon.Api.Infrastructure;
using Aethon.Data;
using Aethon.Data.Identity;
using Aethon.Shared.Auth;
using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IRegistrationProvisioningService _registrationProvisioningService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IRegistrationProvisioningService registrationProvisioningService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _registrationProvisioningService = registrationProvisioningService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var validationErrors = ApiValidationHelper.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(validationErrors);
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (existingUser is not null)
        {
            return ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(RegisterRequestDto.Email)] = ["An account with this email address already exists."]
            });
        }

        var email = request.Email.Trim();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var displayName = $"{firstName} {lastName}".Trim();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = false,
            IsEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ValidationProblem(ApiValidationHelper.FromIdentityErrors(createResult.Errors));
        }

        var provisioningResult = await _registrationProvisioningService.ProvisionAsync(user, request);
        if (!provisioningResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return ValidationProblem(provisioningResult.Errors);
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var apiBaseUrl = (_configuration["Api:BaseUrl"] ?? _configuration["ASPNETCORE_URLS"]?.Split(';').FirstOrDefault() ?? "http://localhost:5201").TrimEnd('/');
        var confirmationUrl = QueryHelpers.AddQueryString(
            $"{apiBaseUrl}/auth/confirm-email",
            new Dictionary<string, string?>
            {
                ["userId"] = user.Id.ToString(),
                ["token"] = token
            });

        _logger.LogInformation("Email confirmation link for {Email}: {ConfirmationUrl}", email, confirmationUrl);

        return Ok(new RegisterResultDto
        {
            Succeeded = true,
            RequiresEmailConfirmation = true,
            Email = email,
            DisplayName = displayName,
            RegistrationType = request.RegistrationType.Trim().ToLowerInvariant()
        });
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var webBaseUrl = (_configuration["Web:BaseUrl"] ?? "http://localhost:5101").TrimEnd('/');

        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Redirect($"{webBaseUrl}/register/confirmed?status=invalid");
        }

        var user = await _userManager.FindByIdAsync(parsedUserId.ToString());
        if (user is null)
        {
            return Redirect($"{webBaseUrl}/register/confirmed?status=invalid");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return Redirect($"{webBaseUrl}/register/confirmed?status=invalid");
        }

        return Redirect($"{webBaseUrl}/register/confirmed?status=success");
    }

    [HttpPost("browser-login")]
    [AllowAnonymous]
    public async Task<IActionResult> BrowserLogin()
    {
        var form = await Request.ReadFormAsync();

        var email = form["email"].ToString().Trim();
        var password = form["password"].ToString();
        var rememberMe = string.Equals(form["rememberMe"], "on", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(form["rememberMe"], "true", StringComparison.OrdinalIgnoreCase);

        var webBaseUrl = (_configuration["Web:BaseUrl"] ?? "http://localhost:5101").TrimEnd('/');
        var returnPath = AuthRedirectHelper.NormaliseReturnPath(form["returnPath"].ToString());

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect(AuthRedirectHelper.BuildLoginRedirect(webBaseUrl, returnPath, "Please enter your email and password."));
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsEnabled)
        {
            return Redirect(AuthRedirectHelper.BuildLoginRedirect(webBaseUrl, returnPath, "Invalid email or password."));
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);

        if (result.IsNotAllowed)
        {
            return Redirect(AuthRedirectHelper.BuildLoginRedirect(webBaseUrl, returnPath, "Please confirm your email address before signing in."));
        }

        if (!result.Succeeded)
        {
            return Redirect(AuthRedirectHelper.BuildLoginRedirect(webBaseUrl, returnPath, "Invalid email or password."));
        }

        return Redirect($"{webBaseUrl}{returnPath}");
    }

    [HttpPost("browser-logout")]
    [AllowAnonymous]
    public async Task<IActionResult> BrowserLogout()
    {
        var form = await Request.ReadFormAsync();

        var webBaseUrl = (_configuration["Web:BaseUrl"] ?? "http://localhost:5101").TrimEnd('/');
        var returnPath = AuthRedirectHelper.NormaliseReturnPath(form["returnPath"].ToString(), "/login");

        await _signInManager.SignOutAsync();

        return Redirect($"{webBaseUrl}{returnPath}");
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new AuthResultDto
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            DisplayName = User.FindFirstValue(AppClaimTypes.DisplayName),
            TenantId = User.FindFirstValue(AppClaimTypes.TenantId),
            TenantSlug = User.FindFirstValue(AppClaimTypes.TenantSlug),
            AppType = User.FindFirstValue(AppClaimTypes.AppType),
            OrganisationId = User.FindFirstValue(AppClaimTypes.OrganisationId),
            OrganisationName = User.FindFirstValue(AppClaimTypes.OrganisationName),
            OrganisationType = User.FindFirstValue(AppClaimTypes.OrganisationType),
            IsOrganisationOwner = string.Equals(
                User.FindFirstValue(AppClaimTypes.IsOrganisationOwner),
                "true",
                StringComparison.OrdinalIgnoreCase),
            CompanyRole = User.FindFirstValue(AppClaimTypes.CompanyRole),
            RecruiterRole = User.FindFirstValue(AppClaimTypes.RecruiterRole),
            HasJobSeekerProfile = string.Equals(
                User.FindFirstValue(AppClaimTypes.HasJobSeekerProfile),
                "true",
                StringComparison.OrdinalIgnoreCase),
            Roles = User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
        });
    }

    [HttpGet("/applications/me")]
    [Authorize]
    public async Task<IActionResult> MyApplications([FromServices] AethonDbContext dbContext)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        var items = await dbContext.JobApplications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SubmittedUtc)
            .Select(x => new JobApplicationListItemDto
            {
                Id = x.Id,
                JobId = x.JobId,
                JobTitle = x.Job.Title,
                OrganisationName = x.Job.OwnedByOrganisation.Name,
                Status = x.Status.ToString(),
                SubmittedUtc = x.SubmittedUtc
            })
            .ToListAsync();

        return Ok(items);
    }

    private ObjectResult ValidationProblem(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(error.Key, string.Join(" ", error.Value));
        }

        return (ObjectResult)ValidationProblem(ModelState);
    }
}