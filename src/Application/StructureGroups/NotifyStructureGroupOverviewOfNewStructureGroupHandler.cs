using Kundenportal.AdminUi.Application.Hubs;
using Kundenportal.AdminUi.Application.Models;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public class NotifyStructureGroupOverviewOfNewStructureGroupHandler(
    IHubContext<StructureGroupHub> hubContext,
    ILogger<NotifyStructureGroupOverviewOfNewStructureGroupHandler> logger)
    : IConsumer<StructureGroupCreated>
{
    private readonly IHubContext<StructureGroupHub> _hubContext = hubContext;
    private readonly ILogger<NotifyStructureGroupOverviewOfNewStructureGroupHandler> _logger = logger;

    public async Task Consume(ConsumeContext<StructureGroupCreated> context)
    {
        try
        {
            StructureGroup structureGroup = context.Message.Adapt<StructureGroup>();
            await _hubContext.Clients.All.SendAsync(
                StructureGroupHub.NewStructureGroupMethod,
                structureGroup,
                context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to the structure group hub");
        }
    }
}
