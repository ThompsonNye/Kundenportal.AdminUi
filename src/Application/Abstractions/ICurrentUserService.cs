using Kundenportal.AdminUi.Application.Models;

namespace Kundenportal.AdminUi.Application.Abstractions;

public interface ICurrentUserService
{
    Task<ApplicationUser> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}