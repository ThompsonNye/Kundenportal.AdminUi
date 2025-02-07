using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;

namespace Kundenportal.AdminUi.Application;

public interface IStructureGroupsService
{
    Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class StructureGroupsService(IApplicationDbContext dbContext)
    : IStructureGroupsService
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        StructureGroup[] structureGroups = _dbContext.StructureGroups.ToArray();
        return Task.FromResult<IEnumerable<StructureGroup>>(structureGroups);
    }
}
