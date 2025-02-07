using Ardalis.Result;
using FluentValidation;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Models.Exceptions;
using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.WebApp.Endpoints.Models.StructureGroups;
using Kundenportal.AdminUi.WebApp.Resources;
using Mapster;
using Polly.Timeout;
using System.Globalization;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Kundenportal.AdminUi.WebApp.Endpoints;

public sealed class CreateStructureGroupEndpoint
{
	private readonly IStructureGroupsService _structureGroupsService;
	private readonly IEnumerable<IValidator<CreateStructureGroupRequest>> _validators;
	private readonly ILogger<CreateStructureGroupEndpoint> _logger;

	public CreateStructureGroupEndpoint(
		IStructureGroupsService structureGroupsService,
		IEnumerable<IValidator<CreateStructureGroupRequest>> validators,
		ILogger<CreateStructureGroupEndpoint> logger)
	{
		_structureGroupsService = structureGroupsService;
		_validators = validators;
		_logger = logger;
	}


	public async Task<IResult> CreateStructureGroupAsync(CreateStructureGroupRequest request, CancellationToken cancellationToken)
	{
		try
		{
			PendingStructureGroup pendingStructureGroup = request.Adapt<PendingStructureGroup>();
			Result result =
				await _structureGroupsService.AddPendingAsync(pendingStructureGroup, cancellationToken);

			if (result.Status == ResultStatus.Conflict)
			{
				Texts.Culture = new CultureInfo("en-US");
				return Results.ValidationProblem(new Dictionary<string, string[]>
				{
					[nameof(CreateStructureGroupRequest.Name)] = [Texts.ValidationErrorStructureGroupExists]
				});
			}

			return Results.Accepted();
		}
		catch (Exception ex)
			when (ex is TimeoutRejectedException or NextcloudRequestException { StatusCode: null or >= 500 })
		{
			_logger.LogWarning("Nextcloud unreachable: {Message}", ex.Message);
			return Results.Problem(title: "Nextcloud unreachable", statusCode: 500);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create structure group");
			return Results.Problem(statusCode: 500);
		}
	}
}
