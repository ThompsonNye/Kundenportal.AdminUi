using MassTransit;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public class StructureGroupCreated : CorrelatedBy<Guid>
{
	public required Guid Id { get; set; }

	public required string Name { get; set; }

	public Guid CorrelationId => Id;
}
