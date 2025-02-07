using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Filters;
using Kundenportal.AdminUi.Infrastructure.Options;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kundenportal.AdminUi.Infrastructure;

/// <summary>
/// The extension methods for configuring the infrastructure related services in the Dependency Injection container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds all the infrastructure related services to the Dependency Injection container.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationDbContext(configuration);

        services.AddMessaging(configuration);

        return services;
    }

    /// <summary>
    /// Configures the DbContext using the connection string from the configuration. Configures the DbContext to use PostgreSQL.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Configures MassTransit for async messaging with RabbitMq as transport.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(IApplicationMarker).Assembly);
            
            x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(5);
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
                o.UsePostgres()
                    .UseBusOutbox();
            });

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
            new Uri($"rabbitmq://{rabbitMqOptions.Host}:{rabbitMqOptions.Port}"),
            rabbitMqOptions.VirtualHost,
            h =>
            {
                h.Username(rabbitMqOptions.Username);
                h.Password(rabbitMqOptions.Password);
            });
    }
}