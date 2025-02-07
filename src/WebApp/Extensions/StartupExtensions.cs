using System.Globalization;
using Asp.Versioning.ApiExplorer;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Infrastructure;
using Kundenportal.AdminUi.WebApp.Components;
using Kundenportal.AdminUi.WebApp.Components.Middleware;
using Kundenportal.AdminUi.WebApp.Endpoints;
using Microsoft.AspNetCore.Localization;

namespace Kundenportal.AdminUi.WebApp.Extensions;

public static class StartupExtensions
{
	public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
	{
		builder.Configuration.AddEnvironmentVariables("Kundenportal_AdminUi_");

		builder.Services.AddApplicationServices();
		builder.Services.AddInfrastructureServices(builder.Configuration);
		builder.Services.AddWebAppServices();

		WebApplication app = builder.Build();
		return app;
	}

	public static WebApplication ConfigurePipeline(this WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.UseMigrationsEndPoint();
		}
		else
		{
			app.UseResponseCompression();
			app.UseExceptionHandler("/Error", true);
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseRequestLocalization(o =>
		{
			o.SupportedCultures =
			[
				new CultureInfo("de-DE"),
				new CultureInfo("en-US")
			];

			o.SupportedUICultures = o.SupportedCultures;

			CultureInfo defaultCulture = o.SupportedCultures![0];
			o.DefaultRequestCulture = new RequestCulture(defaultCulture);
		});

		app.UseHttpsRedirection();

		app.UseStaticFiles();
		app.UseAntiforgery();

		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		app.MapGet("/test", () => "Test2")
			.RequireAuthorization("API");

		// Add additional endpoints required by the Identity /Account Razor components.
		app
			.MapGroup("/")
			.WithTags("Account")
			.MapAdditionalIdentityEndpoints();
		app
			.MapGroup("/connect")
			.WithTags("Connect")
			.MapIdentityApi<ApplicationUser>();

		app.MapHubs();

		app.UseMiddleware<ExceptionHandlingMiddleware>();
		app.MapAppEndpoints();

		app.MapRedirectOnDefaultPath();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI(o =>
			{
				IReadOnlyList<ApiVersionDescription> apiVersions = app.DescribeApiVersions();
				foreach (ApiVersionDescription apiVersionDescription in apiVersions)
				{
					string url = $"/swagger/{apiVersionDescription.GroupName}/swagger.json";
					string name = apiVersionDescription.GroupName.ToUpperInvariant();
					o.SwaggerEndpoint(url, name);
				}
			});
		}

		return app;
	}
}
