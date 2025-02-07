using Microsoft.AspNetCore.SignalR.Client;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public class ScheduleStructureGroupCreation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }
}
