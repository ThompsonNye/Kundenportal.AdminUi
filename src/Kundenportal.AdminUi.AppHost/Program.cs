using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> appDb = builder.AddPostgres("appdb")
	.WithDataVolume()
	.AddDatabase("kundenportal-adminui");

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("rabbitmq")
	.WithDataVolume()
	.WithManagementPlugin();

const string mailUser = "mailuser";
const string mailPassword = "mailpassword";
builder
	.AddContainer("nextcloud-mail", "docker.io/maildev/maildev", "2.2.1")
	.WithVolume("nextcloud-mail-data", "/var/opt/maildev")
	.WithEnvironment("MAILDEV_MAIL_DIRECTORY", "/var/opt/maildev")
	.WithEnvironment("MAILDEV_INCOMING_USER", mailUser)
	.WithEnvironment("MAILDEV_INCOMING_PASS", mailPassword)
	.WithHttpEndpoint(targetPort: 1080)
	.WithEndpoint(targetPort: 1025, scheme: "smtp");

const string nextcloudDbName = "nextcloud";
const string nextcloudDbUser = "nextcloudDbUser";
const string nextcloudDbPassword = "nextcloudDbPassword";
builder.AddContainer("nextcloud-db", "docker.io/mariadb", "10.6")
	.WithVolume("nextcloud-db-data", "/var/lib/mysql")
	.WithEnvironment("MARIADB_DATABASE", nextcloudDbName)
	.WithEnvironment("MARIADB_USER", nextcloudDbUser)
	.WithEnvironment("MARIADB_PASSWORD", nextcloudDbPassword)
	.WithEnvironment("MARIADB_RANDOM_ROOT_PASSWORD", "true")
	.WithArgs("--transaction-isolation=READ-COMMITTED", "--log-bin=binlog", "--binlog-format=ROW")
	.WithEndpoint(targetPort: 3306);

builder
	.AddContainer("nextcloud-redis", "docker.io/redis", "7.2-alpine")
	.WithEndpoint(targetPort: 6379, scheme: "redis", isExternal: false);

const string nextcloudAdminUsername = "admin";
const string nextcloudAdminPassword = "admin1+.";
builder
	.AddDockerfile("nextcloud", "../../scripts/nextcloud", "Dockerfile.Nextcloud")
	.WithVolume("nextcloud-data", "/var/www/html")
	//.WithReference(nextcloudMail)
	//.WithReference(nextcloudDb)
	//.WithReference(nextcloudRedis)
	// TODO Update
	.WithEnvironment("MYSQL_HOST", "nextcloud-db")
	.WithEnvironment("MYSQL_DATABASE", nextcloudDbName)
	.WithEnvironment("MYSQL_USER", nextcloudDbUser)
	.WithEnvironment("MYSQL_PASSWORD", nextcloudDbPassword)
	.WithEnvironment("NEXTCLOUD_ADMIN_USER", nextcloudAdminUsername)
	.WithEnvironment("NEXTCLOUD_ADMIN_PASSWORD", nextcloudAdminPassword)
	// TODO Update
	.WithEnvironment("SMTP_HOST", "nextcloud-mail")
	.WithEnvironment("SMTP_PORT", "1025")
	.WithEnvironment("SMTP_NAME", mailUser)
	.WithEnvironment("SMTP_PASSWORD", mailPassword)
	.WithEnvironment("MAIL_FROM_ADDRESS", "nextcloud")
	.WithEnvironment("MAIL_DOMAIN", "kundenportal.com")
	// TODO Update
	.WithEnvironment("REDIS_HOST", "nextcloud-redis")
	.WithHttpEndpoint(targetPort: 80)
	.WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<WebApp>("app")
	.WithReference(appDb)
	.WaitFor(appDb)
	.WithReference(rabbitMq)
	.WaitFor(rabbitMq)
	.WithEnvironment("Nextcloud:Host", "http://nextcloud")
	.WithEnvironment("Nextcloud:Username", nextcloudAdminUsername)
	.WithEnvironment("Nextcloud:Password", nextcloudAdminPassword);

builder.Build().Run();