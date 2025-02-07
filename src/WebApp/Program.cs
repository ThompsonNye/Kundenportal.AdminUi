using Asp.Versioning.ApiExplorer;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Infrastructure;
using Kundenportal.AdminUi.WebApp;
using Kundenportal.AdminUi.WebApp.Components;
using Kundenportal.AdminUi.WebApp.Components.Middleware;
using Kundenportal.AdminUi.WebApp.Endpoints;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddEnvironmentVariables("Kundenportal_AdminUi_");

builder.AddOpenTelemetry();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebAppServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

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

app.Run();
