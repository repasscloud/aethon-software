using Aethon.Api.Auth;
using Aethon.Api.Endpoints;
using Aethon.Api.Middleware;
using Aethon.Application.Common.Validation;
using Aethon.Application.DependencyInjection;
using Aethon.Data;
using Aethon.Data.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddDbContext<AethonDbContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
});

services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AethonDbContext>();

services.AddAethonAuth(configuration);

services.AddScoped<JwtTokenService>();

services.AddApplicationServices();

services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints();

app.Run();
