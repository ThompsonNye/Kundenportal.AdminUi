using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kundenportal.AdminUi.WebApp.Endpoints.OpenApi;

/// <summary>
///     Registers each api version as its own swagger document.
/// </summary>
/// <param name="versionDescriptionProvider"></param>
public class ConfigureSwaggerGenOptions(
	IApiVersionDescriptionProvider versionDescriptionProvider)
	: IConfigureNamedOptions<SwaggerGenOptions>
{
	private readonly IApiVersionDescriptionProvider _versionDescriptionProvider = versionDescriptionProvider;

	public void Configure(SwaggerGenOptions options)
	{
		foreach (ApiVersionDescription description in _versionDescriptionProvider.ApiVersionDescriptions)
		{
			OpenApiSecurityScheme securityScheme = new()
			{
				Name = "Bearer",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				Reference = new OpenApiReference
				{
					Id = IdentityConstants.BearerScheme,
					Type = ReferenceType.SecurityScheme
				}
			};
			options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{ securityScheme, Array.Empty<string>() }
			});

			OpenApiInfo openApiInfo = new()
			{
				Title = ThisAssembly.RootNamespace
					.Replace(ThisAssembly.AssemblyName, "")
					.Trim('.'),
				Version = description.ApiVersion.ToString()
			};

			options.SwaggerDoc(description.GroupName, openApiInfo);
		}
	}

	public void Configure(string? name, SwaggerGenOptions options)
	{
		Configure(options);
	}
}
