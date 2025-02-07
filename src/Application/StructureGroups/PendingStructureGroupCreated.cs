using MassTransit;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public class PendingStructureGroupCreated : CorrelatedBy<Guid>
{
	public required Guid Id { get; set; }

	public required string Name { get; set; }

	public Guid CorrelationId => Id;
}
