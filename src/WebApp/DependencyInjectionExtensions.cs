using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Infrastructure;
using Kundenportal.AdminUi.WebApp.Components.Pages.Structures;
using Kundenportal.AdminUi.WebApp.Services;

namespace Kundenportal.AdminUi.WebApp;

public static class DependencyInjectionExtensions
{
    public static void MapRedirectOnDefaultPath(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => TypedResults.LocalRedirect($"/{StructureGroups.Route}"));
    }
    
    public static void AddWebAppServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<StructureGroupExplanationService>();

        services.AddValidatorsFromAssemblies([
            typeof(IApplicationMarker).Assembly,
            typeof(IInfrastructureMarker).Assembly,
            typeof(IWebAppMarker).Assembly
        ]);
    }
}