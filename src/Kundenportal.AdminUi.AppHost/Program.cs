using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> appDb = builder.AddPostgres("appdb")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent)
	.AddDatabase("kundenportal-adminui");

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("rabbitmq")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent)
	.WithManagementPlugin();

const string mailUser = "mailuser";
const string mailPassword = "mailpassword";
IResourceBuilder<ContainerResource> nextcloudMail = builder.AddContainer("nextcloud-mail", "docker.io/maildev/maildev", "2.2.1")
	.WithVolume("nextcloud-mail-data", "/var/opt/maildev")
	.WithLifetime(ContainerLifetime.Persistent)
	.WithEnvironment("MAILDEV_MAIL_DIRECTORY", "/var/opt/maildev")
	.WithEnvironment("MAILDEV_INCOMING_USER", mailUser)
	.WithEnvironment("MAILDEV_INCOMING_PASS", mailPassword)
	.WithHttpEndpoint(targetPort: 1080)
	.WithEndpoint(targetPort: 1025, scheme: "smtp");

const string nextcloudDbName = "nextcloud";
const string nextcloudDbUser = "nextcloudDbUser";
const string nextcloudDbPassword = "nextcloudDbPassword";
IResourceBuilder<ContainerResource> nextcloudDb = builder.AddContainer("nextcloud-db", "docker.io/mariadb", "10.6")
	.WithVolume("nextcloud-db-data", "/var/lib/mysql")
	.WithLifetime(ContainerLifetime.Persistent)
	.WithEnvironment("MARIADB_DATABASE", nextcloudDbName)
	.WithEnvironment("MARIADB_USER", nextcloudDbUser)
	.WithEnvironment("MARIADB_PASSWORD", nextcloudDbPassword)
	.WithEnvironment("MARIADB_RANDOM_ROOT_PASSWORD", "true")
	.WithArgs("--transaction-isolation=READ-COMMITTED", "--log-bin=binlog", "--binlog-format=ROW");

IResourceBuilder<ContainerResource> nextcloudRedis = builder.AddContainer("nextcloud-redis", "docker.io/redis", "7.2-alpine")
	.WithLifetime(ContainerLifetime.Persistent)
	.WithEndpoint(targetPort: 6379, scheme: "redis", isExternal: false);

var contextPath = Path.GetFullPath("../../scripts/nextcloud");
var dockerfilePath = Path.GetFullPath("../../Dockerfile.Nextcloud");

IResourceBuilder<ContainerResource> nextcloud = builder.AddDockerfile("nextcloud", contextPath, dockerfilePath)
	.WithVolume("nextcloud-data", "/var/www/html")
	.WithReference(nextcloudDb)
	.WithReference(nextcloudRedis)
	// TODO Update
	.WithEnvironment("MYSQL_HOST", "nextcloud.db")
	.WithEnvironment("MYSQL_DATABASE", nextcloudDbName)
	.WithEnvironment("MYSQL_USER", nextcloudDbUser)
	.WithEnvironment("MYSQL_PASSWORD", nextcloudDbPassword)
	.WithEnvironment("NEXTCLOUD_ADMIN_USER", "admin")
	.WithEnvironment("NEXTCLOUD_ADMIN_PASSWORD", "admin1+.}")
	// TODO Update
	.WithEnvironment("SMTP_HOST", "mail")
	.WithEnvironment("SMTP_PORT", "1025")
	.WithEnvironment("SMTP_NAME", mailUser)
	.WithEnvironment("SMTP_PASSWORD", mailPassword)
	.WithEnvironment("MAIL_FROM_ADDRESS", "nextcloud")
	.WithEnvironment("MAIL_DOMAIN", "kundenportal.com")
	// TODO Update
	.WithEnvironment("REDIS_HOST", "nextcloud.redis");

builder.AddProject<WebApp>("app")
	.WithReference(appDb)
	.WithReference(rabbitMq)
	.WaitFor(appDb)
	.WaitFor(rabbitMq);

builder.Build().Run();