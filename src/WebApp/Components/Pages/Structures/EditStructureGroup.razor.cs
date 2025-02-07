using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.WebApp.Resources;
using MassTransit;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

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

    private readonly Model _model = new();

    private async Task OnValidSubmitAsync()
    {
        Logger!.LogInformation("Name: {Name}", _model.Name);
        await PublishEndpoint!.Publish<CreateStructureGroup.Command>(new
        {
            _model.Name
        });
        NavigationManager!.NavigateTo(StructureGroups.Route);
    }

    public class Model
    {
        [Required(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = nameof(Texts.ValidationErrorFieldRequired))]
        [Display(Name = nameof(Texts.LabelEditStructureGroupName), ResourceType = typeof(Texts))]
        public string Name { get; set; } = "";
    }
}