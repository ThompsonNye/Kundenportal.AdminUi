using Kundenportal.AdminUi.Application.Models;
using Microsoft.AspNetCore.SignalR;

namespace Kundenportal.AdminUi.Application.Hubs;

public class StructureGroupHub : Hub
{
    public const string Route = "/structure-groups";
    
    public const string NewPendingStructureGroupMethod = "NewPendingStructureGroup";
    
    public const string NewStructureGroupMethod = "NewStructureGroup";
    
    public async Task NotifyOfNewPendingStructureGroupAsync(PendingStructureGroup pendingStructureGroup)
    {
        await Clients.All.SendAsync(NewPendingStructureGroupMethod, pendingStructureGroup);
    }
    
    public async Task NotifyOfNewStructureGroupAsync(StructureGroup pendingStructureGroup)
    {
        await Clients.All.SendAsync(NewStructureGroupMethod, pendingStructureGroup);
    }
}