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

    public class Handler(
        ILogger<Handler> logger)
        : IConsumer<Command>
    {
        private readonly ILogger<Handler> _logger = logger;

        public Task Consume(ConsumeContext<Command> context)
        {
            _logger.LogDebug("Creating a structure group with id {Id} and name {Name}", context.Message.Id, context.Message.Name);

            _logger.LogWarning("Not implemented yet");

            return Task.CompletedTask;
        }
    }
}