using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Kundenportal.AdminUi.WebApp.Endpoints.OpenApi;

/// <summary>
/// Registers each api version as its own swagger document.
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
