using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Infrastructure;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using Kundenportal.AdminUi.WebApp.Components.Account;
using Kundenportal.AdminUi.WebApp.Components.Pages.Structures;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Kundenportal.AdminUi.WebApp;

/// <summary>
/// The extension methods for configuring the WebApp related services in the Dependency Injection container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Add a redirect for the path / to the page which should basically serve as the starting point for the UI
    /// unless a specific path is requested.
    /// </summary>
    /// <param name="app"></param>
    public static void MapRedirectOnDefaultPath(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => TypedResults.LocalRedirect($"/{StructureGroups.Route}"));
    }

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
        ]);

        services.AddDefaultWebAppServices();
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
}
