using System.Net.Http.Headers;
using System.Text;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Kundenportal.AdminUi.Application.StructureGroups;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebDav;

namespace Kundenportal.AdminUi.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStructureGroupsService, StructureGroupsService>();
        services.AddScoped<INextcloudApi, NextcloudApi>();

        services.AddMapster();

        services.AddOptions<NextcloudOptions>()
            .BindConfiguration(NextcloudOptions.SectionName)
            .ValidateFluently()
            .ValidateOnStart();

        services.AddHttpClient(Constants.NextcloudHttpClientName, (sp, httpClient) =>
        {
            IOptions<NextcloudOptions> nextcloudOptions = sp.GetRequiredService<IOptions<NextcloudOptions>>();

            httpClient.BaseAddress = new Uri(nextcloudOptions.Value.Host);

            string authenticationHeaderValue = $"{nextcloudOptions.Value.Username}:{nextcloudOptions.Value.Password}";
            authenticationHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationHeaderValue));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authenticationHeaderValue);
        });

        services.AddScoped<IWebDavClient>(sp =>
        {
            IHttpClientFactory httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            HttpClient nextcloudHttpClient = httpClientFactory.CreateClient(Constants.NextcloudHttpClientName);

            WebDavClient webDavClient = new(nextcloudHttpClient);
            return webDavClient;
        });
        
        return services;
    }
}