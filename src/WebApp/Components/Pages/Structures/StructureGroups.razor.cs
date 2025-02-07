using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.StructureGroups;
using Microsoft.AspNetCore.Components;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class StructureGroups
{
    public const string Route = "structure-groups";

    [Inject] public ILogger<StructureGroups>? Logger { get; set; }

    [Inject] public NavigationManager? NavigationManager { get; set; }

    [Inject] public IStructureGroupsService? StructureGroupsService { get; set; }

    private readonly Model _model = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadStructureGroupsAsync();
    }

    private async Task LoadStructureGroupsAsync()
    {
        _model.StructureGroups = (await StructureGroupsService!.GetAllAsync()).ToArray();
        _model.PendingStructureGroups = (await StructureGroupsService!.GetPendingAsync()).ToArray();
    }

    private void OnEditStructureGroupClicked(Guid structureGroupId)
    {
        NavigationManager!.NavigateTo($"{EditStructureGroup.RouteEditBase}/{structureGroupId}");
    }

    private void OnCreateStructureClicked()
    {
        Logger!.LogInformation("Create Structure Option ausgewählt");
    }

    public class Model
    {
        public ICollection<StructureGroup> StructureGroups { get; set; } = Array.Empty<StructureGroup>();
        
        public ICollection<PendingStructureGroup> PendingStructureGroups { get; set; } = Array.Empty<PendingStructureGroup>();
    }
}
