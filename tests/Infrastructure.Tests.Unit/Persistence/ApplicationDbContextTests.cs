using FluentAssertions;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Unit.Persistence;

public class ApplicationDbContextTests
{
	[Fact]
	public void HasPendingModelChanges()
	{
		// Arrange
		using ApplicationDbContext dbContext = new(new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseNpgsql("Host=dummy")
			.Options);

		// Act
		var hasPendingChanges = dbContext.Database.HasPendingModelChanges();

		// Assert
		hasPendingChanges.Should().BeFalse(
			"there should be no model changes pending (consider scaffolding a migration)");
	}
}
