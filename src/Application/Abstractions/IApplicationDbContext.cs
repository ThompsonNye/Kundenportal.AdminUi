using Kundenportal.AdminUi.Application.Models;

namespace Kundenportal.AdminUi.Application.Abstractions;

public interface IApplicationDbContext
{
    IQueryable<StructureGroup> StructureGroups { get; }
    
    IQueryable<UserPreferences> UserPreferences { get; }
}
