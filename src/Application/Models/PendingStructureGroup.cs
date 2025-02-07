using MassTransit;

namespace Kundenportal.AdminUi.Application.Models;

public sealed class PendingStructureGroup
{
    public Guid? Id { get; set; } = NewId.NextSequentialGuid();

    public string Name { get; set; } = "";

    public string Path { get; set; } = "";
}
