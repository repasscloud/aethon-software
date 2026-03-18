using Aethon.Application.Applications.Commands.SubmitJobApplication;
using Aethon.Application.Applications.Queries.GetApplicationById;
using Aethon.Application.Jobs.Commands.CreateJob;
using Aethon.Application.Jobs.Queries.GetJobById;
using Aethon.Application.Organisations.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aethon.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<OrganisationAccessService>();

        services.AddScoped<CreateJobHandler>();
        services.AddScoped<GetJobByIdHandler>();
        services.AddScoped<SubmitJobApplicationHandler>();
        services.AddScoped<GetApplicationByIdHandler>();

        return services;
    }
}
