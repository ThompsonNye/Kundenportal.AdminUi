﻿using Kundenportal.AdminUi.WebApp.Resources;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class EditStructureGroup
{
    public const string RouteEditBase = $"{StructureGroups.Route}/edit";
    public const string RouteEdit = $"{RouteEditBase}/{{id:int}}";
    public const string RouteCreate = $"{StructureGroups.Route}/create";

    [Parameter] public int? Id { get; set; }

    [Inject]
    public ILogger<EditStructureGroup>? Logger { get; set; }

    private readonly Model _model = new();

    private void OnValidSubmit()
    {
        Logger!.LogInformation("Name: {Name}", _model.Name);
    }

    public class Model
    {
        [Required(ErrorMessageResourceType = typeof(Texts), ErrorMessageResourceName = nameof(Texts.ValidationErrorFieldRequired))]
        [Display(Name = nameof(Texts.LabelEditStructureGroupName), ResourceType = typeof(Texts))]
        public string Name { get; set; } = "";
    }
}