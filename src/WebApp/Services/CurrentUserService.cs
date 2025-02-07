using System.Diagnostics;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Kundenportal.AdminUi.WebApp.Services;

public class CurrentUserService(
    AuthenticationStateProvider authenticationStateProvider,
    UserManager<ApplicationUser> userManager)
    : ICurrentUserService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<ApplicationUser> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        AuthenticationState authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        bool isUserAuthenticated = authenticationState.User.Identity?.IsAuthenticated ?? false;
        if (!isUserAuthenticated)
        {
            ThrowOnNoUser();
            throw new UnreachableException("An exception should have been thrown before this");
        }
        
        ApplicationUser? user = await _userManager.GetUserAsync(authenticationState.User);
        if (user is null)
        {
            ThrowOnNoUser();
            throw new UnreachableException("An exception should have been thrown before this");
        }

        return user;
    }

    private static void ThrowOnNoUser()
    {
        throw new InvalidOperationException("No user available");
    }
}