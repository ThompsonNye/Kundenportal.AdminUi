using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.WebApp.Components.Pages.Shared;
using Kundenportal.AdminUi.WebApp.Resources;
using MassTransit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class EditStructureGroup
{
    public const string RouteEditBase = $"{StructureGroups.Route}/edit";
    public const string RouteEdit = $"{RouteEditBase}/{{id:guid}}";
    public const string RouteCreate = $"{StructureGroups.Route}/create";

    [Parameter] public Guid? Id { get; set; }

    [Inject]
    public ILogger<EditStructureGroup>? Logger { get; set; }

    [Inject]
    public IPublishEndpoint? PublishEndpoint { get; init; }

    [Inject]
    public NavigationManager? NavigationManager { get; init; }

    [Inject]
    public IStructureGroupsService? StructureGroupsService { get; init; }
    
    [Inject]
    public ActivitySource? ActivitySource { get; init; }

    private readonly Model _model = new();
    private readonly EditContext _editContext;
    private readonly ValidationMessageStore _validationMessageStore;

    private string? _errorMessage;
    
    public EditStructureGroup()
    {
        _editContext = new EditContext(_model);
        _validationMessageStore = new ValidationMessageStore(_editContext);
    }

    private bool _submitted = false;

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

        bool customValidationResult = await RunCustomValidationAsync();
        if (!customValidationResult)
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
        
        try
        {
            PendingStructureGroup pendingStructureGroup = new()
            {
                Id = Guid.NewGuid(),
                Name = _model.Name
            };
            await StructureGroupsService!.AddPendingAsync(pendingStructureGroup);
            
            return true;
        }
        catch (Exception ex)
        {
            Logger!.LogError(ex, "Failed to create pending structure group");
            _errorMessage = Texts.GenericErrorMessage;
        }

        return false;
    }

    private async Task<bool> RunCustomValidationAsync()
    {
        using Activity? activity = ActivitySource?.StartActivity("CheckFolderExists");
        activity?.AddTag("structureGroup.name", _model.Name);
        
        bool folderExists = await StructureGroupsService!.DoesStructureGroupExistAsync(_model.Name);

        if (folderExists)
        {
            _validationMessageStore.Add(() => _model.Name, Texts.ValidationErrorStructureGroupExists);
        }

        return !folderExists;
    }

    private void ResetErrorMessage()
    {
        _errorMessage = null;
        StateHasChanged();
    }

    public class Model
    {
        [Required(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = nameof(Texts.ValidationErrorFieldRequired))]
        [Display(Name = nameof(Texts.LabelEditStructureGroupName), ResourceType = typeof(Texts))]
        [NegativeRegularExpression(@"[<>?""\/|\\:\*’#%\.]", ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = nameof(Texts.ValidationErrorEditStructureGroupNameContainsInvalidCharacters))]
        [MaxLength(StructureGroup.MaxLengthName, ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = nameof(Texts.ValidationErrorFieldTooLong))]
        public string Name { get; set; } = "";
    }
}
