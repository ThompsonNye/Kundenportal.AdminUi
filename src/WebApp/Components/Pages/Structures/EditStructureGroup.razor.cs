using Microsoft.AspNetCore.Components;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class EditStructureGroup
{
    public const string RouteEditBase = $"{StructureGroups.Route}/edit";
    public const string RouteEdit = $"{RouteEditBase}/{{id:int}}";
    public const string RouteCreate = $"{StructureGroups.Route}/create";

    [Parameter] public int? Id { get; set; }
}