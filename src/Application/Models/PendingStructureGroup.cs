using MassTransit;

namespace Kundenportal.AdminUi.Application.Models;

public sealed class PendingStructureGroup
{
    public const int MaxLengthName = 64;
    
    public Guid Id { get; set; } = NewId.NextSequentialGuid();

    public string Name { get; set; } = "";
}
