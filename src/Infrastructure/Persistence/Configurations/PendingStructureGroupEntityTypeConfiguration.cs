using Kundenportal.AdminUi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kundenportal.AdminUi.Infrastructure.Persistence.Configurations;

public sealed class PendingStructureGroupEntityTypeConfiguration : IEntityTypeConfiguration<PendingStructureGroup>
{
	public void Configure(EntityTypeBuilder<PendingStructureGroup> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(StructureGroup.MaxLengthName);

		builder.HasIndex(x => new
		{
			x.Name
		}).IsUnique();
	}
}
