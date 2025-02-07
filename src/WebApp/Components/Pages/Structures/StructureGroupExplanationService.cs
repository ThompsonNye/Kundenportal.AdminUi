using System.Security.Claims;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Preferences;
using MassTransit;
using Microsoft.AspNetCore.Components.Authorization;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public class StructureGroupExplanationService(
    ILogger<StructureGroupExplanationService> logger,
    IPublishEndpoint publishEndpoint,
    AuthenticationStateProvider authenticationStateProvider,
    IRequestClient<GetUserPreferences> getUserPreferencesClient)
{
    private readonly ILogger<StructureGroupExplanationService> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
    private readonly IRequestClient<GetUserPreferences> _getUserPreferencesClient = getUserPreferencesClient;

    public bool ShowExplanation { get; private set; } = true;

    public async Task InitializeAsync()
    {
        await GetPreferencesAsync();
    }

    private async Task GetPreferencesAsync()
    {
        try
        {
            Guid userId = await GetUserIdForPreferencesAsync();
            (Task<Response<UserPreferences>> preferencesResult, _) =
                await _getUserPreferencesClient.GetResponse<UserPreferences, UserPreferencesNotFound>(
                    new GetUserPreferences()
                    {
                        UserId = userId
                    });

            if (preferencesResult.IsCompletedSuccessfully)
            {
                Response<UserPreferences> preferencesResponse = await preferencesResult;
                ShowExplanation = !preferencesResponse.Message.HideStructureGroupExplanation;
                return;
            }

            _logger.LogWarning("preferences not found");
        }
        catch (Exception ex)
        {
            LogWarningCouldNotLoadPreferences(ex.Message);
        }
    }

    private void LogWarningCouldNotLoadPreferences(string message)
    {
        _logger.LogWarning("Could not load user preferences: {Message}", message);
    }

    public async Task PersistShowStatusForStructureGroupExplanationAsync(bool showExplanation)
    {
        try
        {
            PersistShowStructureGroupExplanation command = new()
            {
                UserId = await GetUserIdForPreferencesAsync(),
                HideExplanation = !showExplanation
            };

            await _publishEndpoint.Publish(command);
            ShowExplanation = showExplanation;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update preferences to not show={Value} the structure group explanation",
                showExplanation);
        }
    }

    private async ValueTask<Guid> GetUserIdForPreferencesAsync()
    {
        AuthenticationState state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        string userIdentifier = state.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        Guid userId = Guid.Parse(userIdentifier);
        return userId;
    }
}