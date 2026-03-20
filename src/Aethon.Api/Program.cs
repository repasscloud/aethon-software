using System.Text.Json.Serialization;
using Aethon.Api.Auth;
using Aethon.Api.Endpoints;
using Aethon.Api.Infrastructure;
using Aethon.Api.Infrastructure.Caching;
using Aethon.Api.Infrastructure.Files;
using Aethon.Api.Infrastructure.Workers;
using Aethon.Api.Middleware;
using Aethon.Application.Abstractions.Caching;
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

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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
services.AddScoped<IFileStorageService, LocalFileStorageService>();
services.AddScoped<JwtTokenService>();

services.AddHttpClient();
services.AddHostedService<WebhookDeliveryWorker>();

services.AddApplicationServices();
services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

services.AddHealthChecks();

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

app.MapHealthChecks("/health");
app.MapApplicationEndpoints();

app.Run();
