using Microsoft.Extensions.DependencyInjection;

namespace Kundenportal.AdminUi.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStructureGroupsService, StructureGroupsService>();

        return services;
    }
}
