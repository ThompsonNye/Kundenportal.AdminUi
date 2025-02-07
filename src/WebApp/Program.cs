using Kundenportal.AdminUi.Infrastructure.Persistence;
using Kundenportal.AdminUi.WebApp.Extensions;
using Microsoft.EntityFrameworkCore;

WebApplication app = WebApplication.CreateBuilder(args)
	.ConfigureServices()
	.ConfigurePipeline();

using (IServiceScope scope = app.Services.CreateScope())
{
	await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	await dbContext.Database.MigrateAsync();
}

app.Run();