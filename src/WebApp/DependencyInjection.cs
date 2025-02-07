using Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

namespace Kundenportal.AdminUi.WebApp;

public static class DependencyInjection
{
    public static void MapRedirectOnDefaultPath(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => TypedResults.LocalRedirect($"/{StructureGroups.Route}"));
    }
}