using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public interface IStructureGroupsService
{
    Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<PendingStructureGroup>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<bool> DoesStructureGroupExistAsync(string name, CancellationToken cancellationToken = default);
}

public sealed class StructureGroupsService(
    IApplicationDbContext dbContext,
    INextcloudApi nextcloud,
    IOptions<NextcloudOptions> nextcloudOptions)
    : IStructureGroupsService
{
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly INextcloudApi _nextcloud = nextcloud;
    private readonly IOptions<NextcloudOptions> _nextcloudOptions = nextcloudOptions;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(2000, cancellationToken);
            StructureGroup[] structureGroups = await _dbContext.StructureGroups.ToArrayAsync(cancellationToken);
            return structureGroups;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<IEnumerable<PendingStructureGroup>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(2000, cancellationToken);
            PendingStructureGroup[] pendingStructureGroups = await _dbContext.PendingStructureGroups.ToArrayAsync(cancellationToken);
            return pendingStructureGroups;
        }
        finally
        {
            _semaphore.Release();
        }
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
            string path = $"{_nextcloudOptions.Value.StructureBasePath}/{name}";
            
            _ = await _nextcloud.GetFolderDetailsAsync(path, cancellationToken);
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
