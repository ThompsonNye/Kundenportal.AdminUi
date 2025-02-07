using Ardalis.Result;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using FluentValidation;
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
using System.Globalization;
using System.Net;

namespace Kundenportal.AdminUi.WebApp.Endpoints;

public static class EndpointRouteBuilderExtensions
{
	/// <summary>
	///     Add a redirect for the path / to the page which should basically serve as the starting point for the UI
	///     unless a specific path is requested.
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
	///     Maps the SignalR hubs this application uses.
	/// </summary>
	/// <param name="endpointRouteBuilder"></param>
	public static void MapHubs(this IEndpointRouteBuilder endpointRouteBuilder)
	{
		RouteGroupBuilder hubsGroup = endpointRouteBuilder.MapGroup("/hubs");

		hubsGroup.MapHub<StructureGroupHub>(StructureGroupHub.Route);
	}

	/// <summary>
	///     Maps all API endpoints for this application under the prefix /api with an additional version prefix,
	///     i.e. all apis will be registered after the prefix /api/vX
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
			.MapPost("structure-groups", (
				[FromBody] CreateStructureGroupRequest request,
				[FromServices] CreateStructureGroupEndpoint endpoint,
				CancellationToken cancellationToken) =>
			{
				return endpoint.CreateStructureGroupAsync(request, cancellationToken);
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
