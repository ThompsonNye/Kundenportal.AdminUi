using Kundenportal.AdminUi.WebApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace WebApp.Tests.Integration;
public sealed class WebAppFactory : WebApplicationFactory<IWebAppMarker>, IAsyncLifetime
{
	private const string s_dbUser = "user";

	private const string s_dbPassword = "password";

	private const string s_rabbitMqUser = "user";

	private const string s_rabbitMqPassword = "password";

	private readonly PostgreSqlContainer _database = new PostgreSqlBuilder()
		.WithUsername(s_dbUser)
		.WithPassword(s_dbPassword)
		.WithDatabase(s_dbUser)
		.WithImage("docker.io/postgres:16-alpine")
		.Build();

	private readonly RabbitMqContainer _broker = new RabbitMqBuilder()
		.WithImage("masstransit/rabbitmq")
		.WithUsername(s_rabbitMqUser)
		.WithPassword(s_rabbitMqPassword)
		.WithPortBinding(15672, true)
		.Build();

	private readonly NextcloudServer _nextcloudServer = new();
	public NextcloudServer NextcloudServer => _nextcloudServer;

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{

		builder.UseConfiguration(new ConfigurationBuilder()
			.AddInMemoryCollection([
				new KeyValuePair<string, string?>("ConnectionStrings:Database", _database.GetConnectionString()),
				new KeyValuePair<string, string?>("RabbitMq:Host", _broker.Hostname),
				new KeyValuePair<string, string?>("RabbitMq:Port", _broker.GetMappedPublicPort(5672).ToString()),
				new KeyValuePair<string, string?>("RabbitMq:Username", s_rabbitMqUser),
				new KeyValuePair<string, string?>("RabbitMq:Password", s_rabbitMqPassword),
				new KeyValuePair<string, string?>("Nextcloud:Host", NextcloudServer.Url),
				new KeyValuePair<string, string?>("Nextcloud:Username", NextcloudServer.Username),
				new KeyValuePair<string, string?>("Nextcloud:Password", NextcloudServer.Password)
			])
			.Build());

		builder.ConfigureTestServices(services =>
		{
			services.AddHostedService<DbMigrator>();
		});
	}

	public async Task InitializeAsync()
	{
		Task databaseStartup = _database.StartAsync();
		Task brokerStartup = _broker.StartAsync();
		await Task.WhenAll(databaseStartup, brokerStartup);

		// Force the application startup logic
		_ = CreateClient();
	}

	async Task IAsyncLifetime.DisposeAsync()
	{
		await _database.DisposeAsync();
	}
}
