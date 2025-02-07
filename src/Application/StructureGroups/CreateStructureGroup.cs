using MassTransit;
using Microsoft.Extensions.Logging;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public static class CreateStructureGroup
{
    public class Command
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }
    }
    
    public class Handler(ILogger<Handler> logger) : IConsumer<Command>
    {
        private readonly ILogger<Handler> _logger = logger;

        public async Task Consume(ConsumeContext<Command> context)
        {
            _logger.LogInformation("Fake creating a structure group with id {Id} and name {Name}", context.Message.Id, context.Message.Name);
            await Task.Delay(5000, context.CancellationToken);
            _logger.LogInformation("Done, fake created structure group {Id} and name {Name}", context.Message.Id, context.Message.Name);
        }
    }
}