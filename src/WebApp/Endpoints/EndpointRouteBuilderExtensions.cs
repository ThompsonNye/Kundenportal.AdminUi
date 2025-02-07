using System.Net;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using FluentValidation;
using FluentValidation.Results;
using Kundenportal.AdminUi.Application.Hubs;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.WebApp.Components.Pages.Structures;
using Kundenportal.AdminUi.WebApp.Endpoints.Models.StructureGroups;
using Microsoft.AspNetCore.Mvc;

namespace Kundenportal.AdminUi.WebApp.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Add a redirect for the path / to the page which should basically serve as the starting point for the UI
    /// unless a specific path is requested.
    /// </summary>
    /// <param name="app"></param>
    public static void MapRedirectOnDefaultPath(this IEndpointRouteBuilder app)
    {
        app
            .MapGet("/", () => TypedResults.LocalRedirect($"/{StructureGroups.Route}"))
            .Produces((int)HttpStatusCode.Redirect)
            .WithDescription("Redirects to the initial application page");
    }
    
    /// <summary>
    /// Maps the SignalR hubs this application uses.
    /// </summary>
    /// <param name="endpointRouteBuilder"></param>
    public static void MapHubs(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        RouteGroupBuilder hubsGroup = endpointRouteBuilder.MapGroup("/hubs");

        hubsGroup.MapHub<StructureGroupHub>(StructureGroupHub.Route);
    }

    /// <summary>
    /// Maps all API endpoints for this application under the prefix /api with an additional version prefix,
    /// i.e. all apis will be registered after the prefix /api/vX
    /// </summary>
    /// <param name="endpointRouteBuilder"></param>
    public static void MapAppEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ApiVersionSet apiVersionSet = endpointRouteBuilder.NewApiVersionSet()
            .HasApiVersion(1.0)
            .Build();
        
        RouteGroupBuilder versionedApis = endpointRouteBuilder.MapGroup("/api/v{apiVersion:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        versionedApis.MapCreateStructureGroupApi();
    }

    private static void MapCreateStructureGroupApi(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        // TODO Implement logic
        endpointRouteBuilder
            .MapPost("structure-groups", (
                [FromServices] IEnumerable<IValidator<CreateStructureGroupRequest>> validators,
                [FromBody] CreateStructureGroupRequest request) =>
            {
                ValidationResult[] validationResult = validators
                    .Select(x => x.Validate(request))
                    .ToArray();

                if (validationResult.Any(x => !x.IsValid))
                {
                    Dictionary<string, string[]> errors = validationResult.Where(x => !x.IsValid)
                        .SelectMany(x => x.ToDictionary())
                        .ToDictionary();
                    return Results.ValidationProblem(errors);
                }
                
                return Results.Ok();
            })
            .MapToApiVersion(1.0)
            .Produces(200)
            .ProducesValidationProblem()
            .WithTags("Structure Groups");
    }
}
