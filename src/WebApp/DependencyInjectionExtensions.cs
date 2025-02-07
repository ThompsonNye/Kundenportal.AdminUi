using Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

namespace Kundenportal.AdminUi.WebApp;

public static class DependencyInjectionExtensions
{
    public static void MapRedirectOnDefaultPath(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => TypedResults.LocalRedirect($"/{StructureGroups.Route}"));
    }
}