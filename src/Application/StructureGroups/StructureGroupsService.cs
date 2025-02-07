using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Services;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public interface IStructureGroupsService
{
    Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> DoesStructureGroupFolderAlreadyExistAsync(string name, CancellationToken cancellationToken = default);
}

public sealed class StructureGroupsService(IApplicationDbContext dbContext, INextcloudApi nextcloud)
    : IStructureGroupsService
{
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly INextcloudApi _nextcloud = nextcloud;

    public Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        StructureGroup[] structureGroups = _dbContext.StructureGroups.ToArray();
        return Task.FromResult<IEnumerable<StructureGroup>>(structureGroups);
    }

    public async Task<bool> DoesStructureGroupFolderAlreadyExistAsync(string name, CancellationToken cancellationToken = default)
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
}
