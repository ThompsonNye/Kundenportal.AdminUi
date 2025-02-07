using Kundenportal.AdminUi.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kundenportal.AdminUi.Infrastructure.Persistence.Configurations;

public sealed class UserPreferencesEntityTypeConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserPreferences>(x => x.UserId);

        builder.Property(x => x.ShowStructureGroupExplanation)
            .IsRequired();
    }
}
