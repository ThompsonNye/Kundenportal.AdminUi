using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Filters;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Infrastructure.Options;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        services.AddMessaging();

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
    /// <returns></returns>
    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateFluently()
            .ValidateOnStart();
        
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
                ConfigureHost(context, cfg);

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
    /// <param name="context"></param>
    /// <param name="cfg"></param>
    private static void ConfigureHost(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg)
    {
        IOptions<RabbitMqOptions> rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>();
        
        cfg.Host(
            rabbitMqOptions.Value.GetUri(),
            rabbitMqOptions.Value.VirtualHost,
            h =>
            {
                h.Username(rabbitMqOptions.Value.Username);
                h.Password(rabbitMqOptions.Value.Password);
            });
    }
}