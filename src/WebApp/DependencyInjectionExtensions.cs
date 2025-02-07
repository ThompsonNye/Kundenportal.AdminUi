using System.Diagnostics;
using Asp.Versioning;
using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Infrastructure;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using Kundenportal.AdminUi.WebApp.Components.Account;
using Kundenportal.AdminUi.WebApp.Endpoints.OpenApi;
using MassTransit.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Trace;

namespace Kundenportal.AdminUi.WebApp;

/// <summary>
/// The extension methods for configuring the WebApp related services in the Dependency Injection container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds all the WebApp related services to the Dependency Injection container.
    /// </summary>
    /// <param name="services"></param>
    public static void AddWebAppServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies([
            typeof(IApplicationMarker).Assembly,
            typeof(IInfrastructureMarker).Assembly,
            typeof(IWebAppMarker).Assembly
        ], ServiceLifetime.Singleton);

        services.AddSignalR();

        services.AddApiVersioning();

        services.ConfigureOptions<ConfigureSwaggerGenOptions>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDefaultWebAppServices();
    }

    public static void AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        ActivitySource activitySource = new(Constants.AppOtelName);
        builder.Services.AddSingleton(activitySource);

        builder.Services.AddOpenTelemetry()
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter()
                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                    .AddSource(activitySource.Name);
            })
            .WithMetrics();
    }

    private static void AddDefaultWebAppServices(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
    }

    private static void AddApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1);
                o.ApiVersionReader = new UrlSegmentApiVersionReader();
                o.ReportApiVersions = true;
            })
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'V";
                o.SubstituteApiVersionInUrl = true;
            });
    }
}