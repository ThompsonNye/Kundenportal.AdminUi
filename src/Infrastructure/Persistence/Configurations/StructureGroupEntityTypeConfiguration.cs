using Kundenportal.AdminUi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kundenportal.AdminUi.Infrastructure.Persistence.Configurations;

public sealed class StructureGroupEntityTypeConfiguration : IEntityTypeConfiguration<StructureGroup>
{
	public void Configure(EntityTypeBuilder<StructureGroup> builder)
	{
		builder.ToTable("StructureGroups");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(StructureGroup.MaxLengthName);

		builder.Property(x => x.Path)
			.IsRequired()
			.HasMaxLength(StructureGroup.MaxLengthPath);

		builder.HasIndex(x => new
		{
			x.Name,
			x.Path
		}).IsUnique();
	}
}
