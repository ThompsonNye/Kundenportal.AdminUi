﻿using Kundenportal.AdminUi.Application.Hubs;
using Kundenportal.AdminUi.Application.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public class NotifyStructureGroupOverviewOfNewPendingStructureGroupHandler(
    IHubContext<PendingStructureGroupHub> hubContext,
    ILogger<NotifyStructureGroupOverviewOfNewPendingStructureGroupHandler> logger)
    : IConsumer<PendingStructureGroupCreated>
{
    private readonly IHubContext<PendingStructureGroupHub> _hubContext = hubContext;
    private readonly ILogger<NotifyStructureGroupOverviewOfNewPendingStructureGroupHandler> _logger = logger;

    public async Task Consume(ConsumeContext<PendingStructureGroupCreated> context)
    {
        try
        {
            PendingStructureGroup pendingStructureGroup = new PendingStructureGroup
            {
                Id = context.Message.Id,
                Name = context.Message.Name
            };
            await _hubContext.Clients.All.SendAsync(PendingStructureGroupHub.NewPendingStructureGroupMethod, pendingStructureGroup,
                cancellationToken: context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to the pending structure group hub");
        }
    }
}
