using Ardalis.Result;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.WebApp.Components.Pages.Shared;
using Kundenportal.AdminUi.WebApp.Resources;
using MassTransit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class EditStructureGroup
{
	public const string RouteEditBase = $"{StructureGroups.Route}/edit";
	public const string RouteEdit = $"{RouteEditBase}/{{id:guid}}";
	public const string RouteCreate = $"{StructureGroups.Route}/create";
	private readonly EditContext _editContext;

	private readonly Model _model = new();
	private readonly ValidationMessageStore _validationMessageStore;

	private string? _errorMessage;

	private bool _submitted;

	public EditStructureGroup()
	{
		_editContext = new EditContext(_model);
		_validationMessageStore = new ValidationMessageStore(_editContext);
	}

	[Parameter] public Guid? Id { get; set; }

	[Inject] public ILogger<EditStructureGroup>? Logger { get; set; }

	[Inject] public IPublishEndpoint? PublishEndpoint { get; init; }

	[Inject] public NavigationManager? NavigationManager { get; init; }

	[Inject] public IStructureGroupsService? StructureGroupsService { get; init; }

	[Inject] public ActivitySource? ActivitySource { get; init; }

	private async Task OnSubmitAsync()
	{
		try
		{
			_validationMessageStore.Clear();

			_submitted = true;

			await OnSubmitLogicAsync();
		}
		finally
		{
			await InvokeAsync(StateHasChanged);
			_submitted = false;
		}
	}

	private async Task OnSubmitLogicAsync()
	{
		bool dataAnnotationsResult = _editContext.Validate();
		if (!dataAnnotationsResult)
		{
			return;
		}

		bool success = await CreatePendingStructureGroupAsync();
		if (!success)
		{
			return;
		}

		NavigationManager!.NavigateTo(StructureGroups.Route);
	}

	private async Task<bool> CreatePendingStructureGroupAsync()
	{
		using Activity? activity = ActivitySource?.StartActivity("CreateStructureGroup");
		activity?.AddTag("structureGroup.name", _model.Name);

		try
		{
			PendingStructureGroup pendingStructureGroup = new()
			{
				Id = Guid.NewGuid(),
				Name = _model.Name
			};
			Result result = await StructureGroupsService!.AddPendingAsync(pendingStructureGroup);

			if (result.Status == ResultStatus.Conflict)
			{
				_validationMessageStore.Add(() => _model.Name, Texts.ValidationErrorStructureGroupExists);
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			Logger!.LogError(ex, "Failed to create pending structure group");
			_errorMessage = Texts.GenericErrorMessage;
		}

		return false;
	}

	private void ResetErrorMessage()
	{
		_errorMessage = null;
		StateHasChanged();
	}

	public class Model
	{
		[Required(ErrorMessageResourceType = typeof(Texts),
			ErrorMessageResourceName = nameof(Texts.ValidationErrorFieldRequired))]
		[Display(Name = nameof(Texts.LabelEditStructureGroupName), ResourceType = typeof(Texts))]
		[NegativeRegularExpression(@"[<>?""\/|\\:\*’#%\.]", ErrorMessageResourceType = typeof(Texts),
			ErrorMessageResourceName = nameof(Texts.ValidationErrorEditStructureGroupNameContainsInvalidCharacters))]
		[MaxLength(StructureGroup.MaxLengthName, ErrorMessageResourceType = typeof(Texts),
			ErrorMessageResourceName = nameof(Texts.ValidationErrorFieldTooLong))]
		public string Name { get; set; } = "";
	}
}
