using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Models;
using Microsoft.AspNetCore.Components;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class StructureGroups
{
    public const string Route = "structuregroups";
    
    [Inject]
    public IStructureGroupsService? StructureGroupsService { get; set; }

    private readonly Model _model = new();

    protected override async Task OnInitializedAsync()
    {
        _model.StructureGroups = (await StructureGroupsService!.GetAllAsync()).ToArray();
    }

    public class Model
    {
        public ICollection<StructureGroup> StructureGroups { get; set; } = Array.Empty<StructureGroup>();
    }
}
