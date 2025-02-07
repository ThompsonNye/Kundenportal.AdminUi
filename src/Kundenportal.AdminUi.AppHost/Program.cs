using Projects;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresDatabaseResource> appDb = builder.AddPostgres("appdb")
	.WithDataVolume()
	.AddDatabase("kundenportal-adminui");

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("rabbitmq")
	.WithDataVolume()
	.WithManagementPlugin();

builder.AddProject<WebApp>("app")
	.WithReference(appDb)
	.WithReference(rabbitMq)
	.WaitFor(appDb)
	.WaitFor(rabbitMq);

builder.Build().Run();