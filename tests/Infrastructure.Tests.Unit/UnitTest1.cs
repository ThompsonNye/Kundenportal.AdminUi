using FluentAssertions;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Unit
{
    public class UnitTest1
    {
        [Fact]
        public void HasPendingModelChanges()
        {
            // Arrange
            using ApplicationDbContext dbContext = new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Host=dummy")
                .Options);

            // Act
            bool hasPendingChanges = dbContext.Database.HasPendingModelChanges();

            // Assert
            hasPendingChanges.Should().BeFalse(
                because: "there should be no model changes pending (consider scaffolding a migration)");
        }
    }
}