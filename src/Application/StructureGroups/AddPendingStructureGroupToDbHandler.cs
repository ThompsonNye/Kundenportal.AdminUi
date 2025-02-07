using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public class AddPendingStructureGroupToDbHandler(
    ILogger<AddPendingStructureGroupToDbHandler> logger,
    IApplicationDbContext dbContext)
    : IConsumer<CreateStructureGroupCommand>
{
    private readonly ILogger<AddPendingStructureGroupToDbHandler> _logger = logger;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<CreateStructureGroupCommand> context)
    {
        _logger.LogDebug("Creating a structure group with id {Id} and name {Name}", context.Message.Id,
            context.Message.Name);

        _dbContext.PendingStructureGroups.Add(new PendingStructureGroup
        {
            Id = context.Message.Id,
            Name = context.Message.Name
        });
        await _dbContext.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogDebug("Saved pending structure group");

        await context.Publish(new PendingStructureGroupCreated
        {
            Id = context.Message.Id,
            Name = context.Message.Name
        });
    }
}
