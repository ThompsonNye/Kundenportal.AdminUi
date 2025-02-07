using FluentValidation;
using Kundenportal.AdminUi.Application.Filters;
using Kundenportal.AdminUi.Application.Preferences;
using Kundenportal.AdminUi.Application.StructureGroups;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Kundenportal.AdminUi.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStructureGroupsService, StructureGroupsService>();

        services.AddMessaging();
        
        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            
            x.AddConsumers(typeof(IApplicationMarker).Assembly);
            x.AddRequestClient<GetUserPreferences>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseConsumeFilter(typeof(ValidationFilter<>), context);

                cfg.UseDelayedRedelivery(r =>
                {
                    r.Intervals(
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromMinutes(15),
                        TimeSpan.FromMinutes(30));
                    r.Ignore<ValidationException>();
                });
                cfg.UseMessageRetry(r =>
                {
                    r.Incremental(
                        5,
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(5));
                    r.Ignore<ValidationException>();
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
