using Kundenportal.AdminUi.Infrastructure.Persistence;
using Kundenportal.AdminUi.WebApp.Extensions;
using Microsoft.EntityFrameworkCore;

var app = WebApplication.CreateBuilder(args)
	.ConfigureServices()
	.ConfigurePipeline();

using (var scope = app.Services.CreateScope())
{
	await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	await dbContext.Database.MigrateAsync();
}

app.Run();
