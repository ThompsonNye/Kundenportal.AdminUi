using MassTransit;

namespace Kundenportal.AdminUi.Application.Models;

public sealed class StructureGroup
{
    public const int MaxLengthName = 64;

    public const int MaxLengthPath = 256;
    
    public Guid Id { get; set; } = NewId.NextSequentialGuid();

    public string Name { get; set; } = "";

    public string Path { get; set; } = "";

    public Guid? ParentId { get; set; }
}
