using Aethon.Api.Auth;
using Aethon.Api.Endpoints;
using Aethon.Api.Infrastructure;
using Aethon.Api.Infrastructure.Caching;
using Aethon.Api.Middleware;
using Aethon.Application.Abstractions.Caching;
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

services.AddMemoryCache();
services.AddSingleton<IAppCache, MemoryAppCache>();

services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
services.AddScoped<JwtTokenService>();

services.AddApplicationServices();
services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints();

app.Run();
