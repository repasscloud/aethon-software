using Aethon.Api.Auth;
using Aethon.Api.Endpoints;
using Aethon.Api.Infrastructure;
using Aethon.Api.Infrastructure.Files;
using Aethon.Application.Abstractions.Files;
using Aethon.Application.Abstractions.Time;
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
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
});

services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AethonDbContext>();

services.AddAethonAuth(configuration);

services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
services.AddScoped<IFileStorageService, LocalFileStorageService>();
services.AddScoped<JwtTokenService>();

services.AddApplicationServices();

services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints();

app.Run();