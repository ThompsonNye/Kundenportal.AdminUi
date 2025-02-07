using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Filters;
using Kundenportal.AdminUi.Application.Preferences;
using Kundenportal.AdminUi.Infrastructure.Options;
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

        services.AddMessaging(configuration);

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

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(IApplicationMarker).Assembly);
            x.AddRequestClient<GetUserPreferences>();

            x.UsingRabbitMq((context, cfg) =>
            {
                ConfigureHost(cfg, configuration);

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

    /// <summary>
    /// Configures the RabbitMq instance connection details. Retrieves the options to use from configuration or falls back to the default values.
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="configuration"></param>
    private static void ConfigureHost(IRabbitMqBusFactoryConfigurator cfg, IConfiguration configuration)
    {
        RabbitMqOptions rabbitMqOptions = configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>()
            ?? new RabbitMqOptions();

        cfg.Host(
            rabbitMqOptions.Host,
            rabbitMqOptions.VirtualHost,
            h =>
            {
                h.Username(rabbitMqOptions.Username);
                h.Password(rabbitMqOptions.Password);
            });
    }
}