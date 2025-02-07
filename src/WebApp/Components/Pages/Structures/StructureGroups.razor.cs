using Kundenportal.AdminUi.Application.Hubs;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.StructureGroups;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class StructureGroups : IAsyncDisposable
{
    public const string Route = "structure-groups";

    [Inject] public ILogger<StructureGroups>? Logger { get; set; }

    [Inject] public NavigationManager? NavigationManager { get; set; }

    [Inject] public IStructureGroupsService? StructureGroupsService { get; set; }

    private readonly Model _model = new();
    private HubConnection? _pendingStructureGroupsConnection;

    protected override async Task OnInitializedAsync()
    {
        await LoadStructureGroupsAsync();
        await CreateHubConnectionsAsync();
    }

    private async Task LoadStructureGroupsAsync()
    {
        await LoadStructureGroupDataAsync();
        await LoadPendingStructureGroupDataAsync();
    }

    private async Task LoadStructureGroupDataAsync()
    {
        try
        {
            _model.StructureGroups = (await StructureGroupsService!.GetAllAsync()).ToArray();
        }
        catch (Exception ex)
        {
            Logger!.LogError(ex, "Failed to load structure groups");
        }
    }

    private async Task LoadPendingStructureGroupDataAsync()
    {
        try
        {
            _model.PendingStructureGroups = (await StructureGroupsService!.GetPendingAsync()).ToList();
            Logger!.LogDebug("Loaded {Count} pending structure groups from db", _model.PendingStructureGroups.Count);
        }
        catch (Exception ex)
        {
            Logger!.LogError(ex, "Failed to load pending structure groups");
        }
    }

    private async Task CreateHubConnectionsAsync()
    {
        Uri uri = NavigationManager!.ToAbsoluteUri($"/hubs{PendingStructureGroupHub.Route}");
        _pendingStructureGroupsConnection = new HubConnectionBuilder()
            .WithUrl(uri)
            .Build();

        _pendingStructureGroupsConnection.On<PendingStructureGroup>(
            PendingStructureGroupHub.NewPendingStructureGroupMethod, OnNewPendingStructureGroupAsync);

        await _pendingStructureGroupsConnection.StartAsync();
    }

    private void OnEditStructureGroupClicked(Guid structureGroupId)
    {
        NavigationManager!.NavigateTo($"{EditStructureGroup.RouteEditBase}/{structureGroupId}");
    }

    private void OnCreateStructureClicked()
    {
        Logger!.LogInformation("Create Structure Option ausgewählt");
    }

    private async Task OnNewPendingStructureGroupAsync(PendingStructureGroup pendingStructureGroup)
    {
        Logger!.LogDebug("Got new pending structure group");
        
        try
        {
            if (!_model.PendingStructureGroups.Any(x => x.Id == pendingStructureGroup.Id))
            {
                _model.PendingStructureGroups.Add(pendingStructureGroup);
                Logger!.LogDebug("Added new pending structure group to page model");
                await InvokeAsync(StateHasChanged);
                return;
            }

            Logger!.LogDebug("A pending structure group with id {Id} is already present in the page model", pendingStructureGroup.Id);
        }
        catch (Exception ex)
        {
            Logger!.LogError(ex, "Failed to add new pending structure group to page model");
        }
    }

    public class Model
    {
        public ICollection<StructureGroup> StructureGroups { get; set; } = Array.Empty<StructureGroup>();

        // Using a list here because the values can be updates after the fact
        // when receiving a notification via SignalR
        public List<PendingStructureGroup> PendingStructureGroups { get; set; } = [];
    }

    public async ValueTask DisposeAsync()
    {
        if (_pendingStructureGroupsConnection is not null)
        {
            await _pendingStructureGroupsConnection.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }
}
