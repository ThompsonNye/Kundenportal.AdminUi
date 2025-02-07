using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Unit;

public static class InMemoryDbContextProvider
{
	public static IApplicationDbContext GetDbContext()
	{
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new ApplicationDbContext(options);
	}
}
