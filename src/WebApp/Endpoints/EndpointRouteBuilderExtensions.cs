using System.Globalization;
using System.Net;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using FluentValidation;
using FluentValidation.Results;
using Kundenportal.AdminUi.Application.Hubs;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Models.Exceptions;
using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.WebApp.Components.Pages.Structures;
using Kundenportal.AdminUi.WebApp.Endpoints.Models.StructureGroups;
using Kundenportal.AdminUi.WebApp.Endpoints.OpenApi;
using Kundenportal.AdminUi.WebApp.Resources;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Polly.Timeout;

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
        endpointRouteBuilder
            .MapPost("structure-groups", async (
                [FromBody] CreateStructureGroupRequest request,
                [FromServices] IEnumerable<IValidator<CreateStructureGroupRequest>> validators,
                [FromServices] IStructureGroupsService structureGroupsService,
                [FromServices] ILoggerFactory loggerFactory,
                CancellationToken cancellationToken) =>
            {
                ILogger logger = loggerFactory.CreateLogger("StructureGroupApis");
                
                try
                {
                    bool doesStructureGroupExist =
                        await structureGroupsService.DoesStructureGroupExistAsync(request.Name, cancellationToken);
                    if (doesStructureGroupExist)
                    {
                        Texts.Culture = new CultureInfo("en-US");
                        return Results.ValidationProblem(new Dictionary<string, string[]>
                        {
                            [nameof(CreateStructureGroupRequest.Name)] = [Texts.ValidationErrorStructureGroupExists]
                        });
                    }

                    PendingStructureGroup pendingStructureGroup = request.Adapt<PendingStructureGroup>();
                    await structureGroupsService.AddPendingAsync(pendingStructureGroup, cancellationToken);

                    return Results.Accepted();
                }
                catch (Exception ex)
                    when (ex is TimeoutRejectedException or NextcloudRequestException { StatusCode: null or >= 500 })
                {
                    logger.LogWarning("Nextcloud unreachable: {Message}", ex.Message);
                    return Results.Problem(title: "Nextcloud unreachable", statusCode: 500);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create structure group");
                    return Results.Problem(statusCode: 500);
                }
            })
            .AddEndpointFilter<ValidationFilter<CreateStructureGroupRequest>>()
            .RequireAuthorization(p =>
                p.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(IdentityConstants.BearerScheme))
            .MapToApiVersion(1.0)
            .Produces((int)HttpStatusCode.Accepted)
            .ProducesValidationProblem()
            .WithTags(SwaggerDocConstants.StructureGroupTag);
    }
}
