using FluentValidation;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Filters;
using Kundenportal.AdminUi.Infrastructure.Options;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using MassTransit;
using MassTransit.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Kundenportal.AdminUi.Infrastructure;

/// <summary>
///     The extension methods for configuring the infrastructure related services in the Dependency Injection container.
/// </summary>
public static class DependencyInjectionExtensions
{
	/// <summary>
	///     Adds all the infrastructure related services to the Dependency Injection container.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
	{
		builder.AddApplicationDbContext();

		builder.AddMessaging();
	}

	/// <summary>
	///     Configures the DbContext using the connection string from the configuration. Configures the DbContext to use
	///     PostgreSQL.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	private static void AddApplicationDbContext(this IHostApplicationBuilder builder)
	{
		builder.AddNpgsqlDbContext<ApplicationDbContext>("kundenportal-adminui");
		builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
	}

	/// <summary>
	///     Configures MassTransit for async messaging with RabbitMq as transport.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	private static void AddMessaging(this IHostApplicationBuilder builder)
	{
		string? rabbitMqConnectionString = builder.Configuration["ConnectionStrings:rabbitmq"];

		if (string.IsNullOrWhiteSpace(rabbitMqConnectionString))
		{
			throw new InvalidOperationException(
				"RabbitMQ connection string is missing or empty in configuration.");
		}

		builder.Services.AddOpenTelemetry()
			.WithMetrics(b => b.AddMeter(DiagnosticHeaders.DefaultListenerName))
			.WithTracing(providerBuilder =>
			{
				providerBuilder.AddSource(DiagnosticHeaders.DefaultListenerName);
			});

		// builder.Services.AddOptions<RabbitMqOptions>()
		// 	.BindConfiguration(RabbitMqOptions.SectionName)
		// 	.ValidateFluently()
		// 	.ValidateOnStart();

		builder.Services.AddMassTransit(x =>
		{
			x.SetKebabCaseEndpointNameFormatter();
			x.SetInMemorySagaRepositoryProvider();

			x.AddConsumers(typeof(IApplicationMarker).Assembly);

			x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
			{
				o.QueryDelay = TimeSpan.FromSeconds(5);
				o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
				o.UsePostgres()
					.UseBusOutbox();
			});

			x.UsingRabbitMq((context, cfg) =>
			{
				Uri uri = new(rabbitMqConnectionString);
				cfg.Host(uri);

				cfg.UseConsumeFilter(typeof(ValidationFilter<>), context);

				cfg.UseDelayedRedelivery(r =>
				{
					r.Intervals(
						TimeSpan.FromMinutes(5),
						TimeSpan.FromMinutes(15),
						TimeSpan.FromMinutes(30));
					r.Ignore<ValidationException>();
				});
				cfg.UseMessageRetry(r =>
				{
					r.Incremental(
						5,
						TimeSpan.Zero,
						TimeSpan.FromSeconds(5));
					r.Ignore<ValidationException>();
				});

				cfg.ConfigureEndpoints(context);
			});
		});
	}

	/// <summary>
	///     Configures the RabbitMq instance connection details. Retrieves the options to use from configuration or falls back
	///     to the default values.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="cfg"></param>
	private static void ConfigureHost(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator cfg)
	{
		IOptions<RabbitMqOptions> rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>();

		cfg.Host(
			rabbitMqOptions.Value.GetUri(),
			rabbitMqOptions.Value.VirtualHost,
			h =>
			{
				h.Username(rabbitMqOptions.Value.Username);
				h.Password(rabbitMqOptions.Value.Password);
			});
	}
}