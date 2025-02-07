using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Filters;
using Kundenportal.AdminUi.Application.Preferences;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kundenportal.AdminUi.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationDbContext(configuration);

        services.AddMessaging();

        return services;
    }

    private static IServiceCollection AddApplicationDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") ??
                               throw new InvalidOperationException("Database connection string not found.");
        services.AddDbContext<ApplicationDbContext>((services, options) =>
        {
            options.UseNpgsql(connectionString, o =>
            {
                o.EnableRetryOnFailure();
                o.MigrationsAssembly(typeof(IInfrastructureMarker).Assembly.GetName().FullName);
            });
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

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