using MassTransit;
using Microsoft.Extensions.Logging;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public static class CreateStructureGroup
{
    public class Command
    {
        public NewId Id { get; } = NewId.Next();
    }
    
    public class Handler(ILogger<Handler> logger) : IConsumer<Command>
    {
        private readonly ILogger<Handler> _logger = logger;

        public Task Consume(ConsumeContext<Command> context)
        {
            _logger.LogInformation("Received command to create structure group {Id}", context.Message.Id);
            throw new Exception();
            return Task.CompletedTask;
        }
    }
}