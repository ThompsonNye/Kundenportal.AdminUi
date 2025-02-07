namespace Kundenportal.AdminUi.Application.Models;

public class UserPreferences
{
    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    
    public bool HideStructureGroupExplanation { get; set; }
}
