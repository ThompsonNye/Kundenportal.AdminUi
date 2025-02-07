using Kundenportal.AdminUi.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Kundenportal.AdminUi.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<StructureGroup> StructureGroups { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
