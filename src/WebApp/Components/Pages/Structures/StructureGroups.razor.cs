using Kundenportal.AdminUi.Application.Hubs;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.StructureGroups;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Structures;

public partial class StructureGroups : IAsyncDisposable
{
	public const string Route = "structure-groups";

	private readonly Model _model = new();
	private HubConnection? _structureGroupsConnection;

	[Inject] public ILogger<StructureGroups>? Logger { get; set; }

	[Inject] public NavigationManager? NavigationManager { get; set; }

	[Inject] public IStructureGroupsService? StructureGroupsService { get; set; }

	public async ValueTask DisposeAsync()
	{
		if (_structureGroupsConnection is not null) await _structureGroupsConnection.DisposeAsync();

		GC.SuppressFinalize(this);
	}

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
			_model.StructureGroups = (await StructureGroupsService!.GetAllAsync()).ToList();
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
		var uri = NavigationManager!.ToAbsoluteUri($"/hubs{StructureGroupHub.Route}");
		_structureGroupsConnection = new HubConnectionBuilder()
			.WithUrl(uri)
			.Build();

		_structureGroupsConnection.On<PendingStructureGroup>(
			StructureGroupHub.NewPendingStructureGroupMethod, OnNewPendingStructureGroupAsync);

		_structureGroupsConnection.On<StructureGroup>(
			StructureGroupHub.NewStructureGroupMethod, OnNewStructureGroupAsync);

		await _structureGroupsConnection.StartAsync();
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
			if (_model.PendingStructureGroups.Any(x => x.Id == pendingStructureGroup.Id))
			{
				Logger!.LogDebug("A pending structure group with id {Id} is already present in the page model",
					pendingStructureGroup.Id);
				return;
			}

			_model.PendingStructureGroups.Add(pendingStructureGroup);
			Logger!.LogDebug("Added new pending structure group to page model");
			await InvokeAsync(StateHasChanged);
		}
		catch (Exception ex)
		{
			Logger!.LogError(ex, "Failed to add new pending structure group to page model");
		}
	}

	private async Task OnNewStructureGroupAsync(StructureGroup structureGroup)
	{
		Logger!.LogDebug("Got new structure group");

		try
		{
			AddNewStructureGroup(structureGroup);
			RemoveCorrespondingPendingStructureGroup(structureGroup);
		}
		catch (Exception ex)
		{
			Logger!.LogError(ex, "Failed to add new structure group to page model");
		}
		finally
		{
			await InvokeAsync(StateHasChanged);
		}
	}

	private void AddNewStructureGroup(StructureGroup structureGroup)
	{
		if (_model.StructureGroups.Any(x => x.Id == structureGroup.Id))
		{
			Logger!.LogDebug("A structure group with id {Id} is already present in the page model", structureGroup.Id);
			return;
		}

		_model.StructureGroups.Add(structureGroup);
		Logger!.LogDebug("Added new structure group to page model");
	}

	private void RemoveCorrespondingPendingStructureGroup(StructureGroup structureGroup)
	{
		_model.PendingStructureGroups.RemoveAll(x => x.Id == structureGroup.Id);
	}

	public class Model
	{
		// Using lists here because the values can be updates after the fact
		// when receiving a notification via SignalR

		public List<StructureGroup> StructureGroups { get; set; } = [];

		public List<PendingStructureGroup> PendingStructureGroups { get; set; } = [];
	}
}
