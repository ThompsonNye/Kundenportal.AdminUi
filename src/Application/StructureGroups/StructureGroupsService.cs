using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public interface IStructureGroupsService
{
    Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<PendingStructureGroup>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<bool> DoesStructureGroupExistAsync(string name, CancellationToken cancellationToken = default);
}

public sealed class StructureGroupsService(IApplicationDbContext dbContext, INextcloudApi nextcloud)
    : IStructureGroupsService
{
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly INextcloudApi _nextcloud = nextcloud;

    public async Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        StructureGroup[] structureGroups = await _dbContext.StructureGroups.ToArrayAsync(cancellationToken);
        return structureGroups;
    }
    
    public async Task<IEnumerable<PendingStructureGroup>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        PendingStructureGroup[] pendingStructureGroups = await _dbContext.PendingStructureGroups.ToArrayAsync(cancellationToken);
        return pendingStructureGroups;
    }

    public async Task<bool> DoesStructureGroupExistAsync(string name, CancellationToken cancellationToken = default)
    {
        bool pendingGroupExists =
            await DoesAPendingStructureGroupWithThatNameAlreadyExistAsync(name, cancellationToken);

        if (pendingGroupExists)
        {
            return true;
        }
        
        bool existsInNextcloud = await DoesStructureGroupFolderExistInNextcloudAsync(name, cancellationToken);
        return existsInNextcloud;
    }

    private async Task<bool> DoesStructureGroupFolderExistInNextcloudAsync(string name,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO Use structure group base folder to create path
            _ = await _nextcloud.GetFolderDetailsAsync(name, cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            // TODO 
            Console.WriteLine(e);
            return false;
        }
    }

    private async Task<bool> DoesAPendingStructureGroupWithThatNameAlreadyExistAsync(string name,
        CancellationToken cancellationToken = default)
    {
        bool exists = await _dbContext
            .PendingStructureGroups
            .AnyAsync(x => x.Name == name, cancellationToken);

        return exists;
    }
}
