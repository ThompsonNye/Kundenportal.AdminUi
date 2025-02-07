﻿using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Infrastructure.Persistence;
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
}