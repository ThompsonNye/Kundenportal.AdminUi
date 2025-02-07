using Kundenportal.AdminUi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApp.Tests.Integration;

public sealed class DbMigrator : IHostedService
{
	private readonly IServiceScopeFactory _scopeFactory;

	public DbMigrator(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using IServiceScope scope = _scopeFactory.CreateScope();
		using ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await db.Database.MigrateAsync(cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
