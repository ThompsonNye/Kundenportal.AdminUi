using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text;
using Kundenportal.AdminUi.Application.Models.Exceptions;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Kundenportal.AdminUi.Application.StructureGroups;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
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
				var nextcloudOptions = sp.GetRequiredService<IOptions<NextcloudOptions>>();

				httpClient.BaseAddress = new Uri(nextcloudOptions.Value.Host);

				var authenticationHeaderValue =
					$"{nextcloudOptions.Value.Username}:{nextcloudOptions.Value.Password}";
				authenticationHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationHeaderValue));
				httpClient.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Basic", authenticationHeaderValue);
			})
			.AddResilienceHandler("Retry", (builder, context) =>
			{
				var retryableExceptions = new[]
				{
					typeof(NextcloudRequestException)
				}.ToImmutableArray();

				var nextcloudOptions =
					context.ServiceProvider.GetRequiredService<IOptions<NextcloudOptions>>();
				builder
					.AddRetry(new HttpRetryStrategyOptions
					{
						BackoffType = DelayBackoffType.Exponential,
						Delay = TimeSpan.FromMilliseconds(nextcloudOptions.Value.RetryDelay),
						UseJitter = true,
						ShouldHandle = async response =>
						{
							var handledByDefault = await new HttpRetryStrategyOptions().ShouldHandle(response);
							if (handledByDefault) return true;

							return retryableExceptions.Contains(response.GetType());
						}
					});
			});

		services.AddScoped<IWebDavClient>(sp =>
		{
			var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
			var nextcloudHttpClient = httpClientFactory.CreateClient(Constants.NextcloudHttpClientName);

			WebDavClient webDavClient = new(nextcloudHttpClient);
			return webDavClient;
		});

		return services;
	}
}
