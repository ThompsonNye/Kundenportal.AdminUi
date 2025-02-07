using FluentValidation;
using Kundenportal.AdminUi.Application.Models;

namespace Kundenportal.AdminUi.WebApp.Endpoints.Models.StructureGroups;

public sealed class CreateStructureGroupRequest
{
    public string Name { get; set; } = "";
}

public sealed class CreateStructureGroupRequestValidator : AbstractValidator<CreateStructureGroupRequest>
{
    public CreateStructureGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(StructureGroup.MaxLengthName);
    }
}
