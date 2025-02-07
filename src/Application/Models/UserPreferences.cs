namespace Kundenportal.AdminUi.Application.Models;

public class UserPreferences
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    
    public bool ShowStructureGroupExplanation { get; set; } = true;
}
