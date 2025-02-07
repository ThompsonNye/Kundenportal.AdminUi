using Kundenportal.AdminUi.Application.Models;
using Microsoft.AspNetCore.SignalR;

namespace Kundenportal.AdminUi.Application.Hubs;

public class PendingStructureGroupHub : Hub
{
    public const string Route = "/pending-structure-groups";
    
    public const string NewPendingStructureGroupMethod = "NewPendingStructureGroup";
    
    public async Task NotifyOfNewPendingStructureGroupAsync(PendingStructureGroup pendingStructureGroup)
    {
        await Clients.All.SendAsync(NewPendingStructureGroupMethod, pendingStructureGroup);
    }
}