using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.WebApp.Components.Pages.Shared;
using Kundenportal.AdminUi.WebApp.Resources;
using MassTransit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
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

    private readonly Model _model = new();
    private readonly EditContext _editContext;
    private readonly ValidationMessageStore _validationMessageStore;

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

        await TriggerCreationOfStructureGroupFolderAsync();
        NavigationManager!.NavigateTo(StructureGroups.Route);
    }

    private async Task TriggerCreationOfStructureGroupFolderAsync()
    {
        Logger!.LogInformation("Name: {Name}", _model.Name);
        await PublishEndpoint!.Publish(new CreateStructureGroup.Command
        {
            Name = _model.Name
        });
    }

    private async Task<bool> RunCustomValidationAsync()
    {
        bool folderExists = await StructureGroupsService!.DoesStructureGroupExistAsync(_model.Name);

        if (folderExists)
        {
            _validationMessageStore.Add(() => _model.Name, Texts.ValidationErrorStructureGroupExists);
        }

        return !folderExists;
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
