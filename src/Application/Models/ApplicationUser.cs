using Microsoft.AspNetCore.Identity;

namespace Kundenportal.AdminUi.Application.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser<Guid>;
