using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Kundenportal.AdminUi.Application.StructureGroups;
using Microsoft.Extensions.DependencyInjection;

namespace Kundenportal.AdminUi.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStructureGroupsService, StructureGroupsService>();
        services.AddScoped<INextcloudApi, NextcloudApi>();

        services.AddOptions<NextcloudOptions>()
            .BindConfiguration(NextcloudOptions.SectionName)
            .ValidateFluently()
            .ValidateOnStart();


        return services;
    }
}