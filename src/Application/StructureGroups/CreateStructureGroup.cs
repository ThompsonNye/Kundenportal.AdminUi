using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
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
        ILogger<Handler> logger,
        IApplicationDbContext dbContext)
        : IConsumer<Command>
    {
        private readonly ILogger<Handler> _logger = logger;
        private readonly IApplicationDbContext _dbContext = dbContext;

        public async Task Consume(ConsumeContext<Command> context)
        {
            _logger.LogDebug("Creating a structure group with id {Id} and name {Name}", context.Message.Id, context.Message.Name);

            _dbContext.PendingStructureGroups.Add(new PendingStructureGroup
            {
                Id = context.Message.Id,
                Name = context.Message.Name
            });
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            
            _logger.LogDebug("Saved pending structure group");
            
            _logger.LogWarning("Creating folder in Nextcloud is not implemented yet");
        }
    }
}