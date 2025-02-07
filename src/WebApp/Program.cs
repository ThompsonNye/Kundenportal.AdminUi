using Kundenportal.AdminUi.WebApp.Extensions;

WebApplication.CreateBuilder(args)
    .ConfigureServices()
    .ConfigurePipeline()
    .Run();
