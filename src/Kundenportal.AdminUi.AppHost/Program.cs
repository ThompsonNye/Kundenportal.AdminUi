using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> appDb = builder.AddPostgres("appdb")
	.WithDataVolume()
	.AddDatabase("kundenportal-adminui");

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("rabbitmq")
	.WithDataVolume()
	.WithManagementPlugin();

builder.AddContainer("nextcloud-mail", "docker.io/maildev/maildev", "2.2.1")
	.WithBindMount("VolumeMount.AppHost-nextcloud-mail-data", "/var/opt/maildev")
	.WithEnvironment("MAILDEV_MAIL_DIRECTORY", "/var/opt/maildev")
	.WithEnvironment("MAILDEV_INCOMING_USER", "mailuser")
	.WithEnvironment("MAILDEV_INCOMING_PASS", "mailpassword")
	.WithHttpEndpoint(targetPort: 1080)
	.WithEndpoint(targetPort: 1025, scheme: "smtp");

builder.AddProject<WebApp>("app")
	.WithReference(appDb)
	.WithReference(rabbitMq)
	.WaitFor(appDb)
	.WaitFor(rabbitMq);

builder.Build().Run();