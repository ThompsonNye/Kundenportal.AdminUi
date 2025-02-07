using Ardalis.Result;
using FluentAssertions;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.StructureGroups;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApp.Tests.Integration;

public sealed class CreateStructureGroupTests : IClassFixture<WebAppFactory>, IAsyncLifetime
{
	private readonly IServiceScope _scope;
	private readonly IStructureGroupsService _structureGroupsService;
	private readonly WebAppFactory _webAppFactory;

	public CreateStructureGroupTests(WebAppFactory webAppFactory)
	{
		_webAppFactory = webAppFactory;
		_scope = webAppFactory.Services.CreateScope();
		_structureGroupsService = _scope.ServiceProvider.GetRequiredService<IStructureGroupsService>();
	}

	public Task InitializeAsync()
	{
		return Task.CompletedTask;
	}

	public async Task DisposeAsync()
	{
		_scope.Dispose();
		await _webAppFactory.ResetDatabaseAsync();
	}

	[Fact]
	public async Task AddPendingStructureGroup_ShouldResultInStructureGroupBeingCreated_WhenNoConflictExists()
	{
		// Arrange
		PendingStructureGroup pendingStructureGroup = new() { Name = "Test" };
		IOptions<NextcloudOptions> nextcloudOptions =
			_scope.ServiceProvider.GetRequiredService<IOptions<NextcloudOptions>>();
		string path = nextcloudOptions.Value.CombineWithStructureBasePath(pendingStructureGroup.Name);
		_webAppFactory.NextcloudServer.SetupGetEmptyResources(nextcloudOptions.Value.Username,
			nextcloudOptions.Value.Password, path);
		_webAppFactory.NextcloudServer.SetupCreateFolder(nextcloudOptions.Value.Username,
			nextcloudOptions.Value.Password, path);

		// Act
		Result result = await _structureGroupsService.AddPendingAsync(pendingStructureGroup);

		// Assert
		result.IsSuccess.Should().BeTrue();

		// Wait for the structure group to be created
		for (int i = 0; i < 30; i++)
		{
			StructureGroup[] structureGroups = (await _structureGroupsService.GetAllAsync()).ToArray();
			if (structureGroups.Any(x => x.Id == pendingStructureGroup.Id))
			{
				// Test method can exit now
				return;
			}

			await Task.Delay(1000);
		}

		// At this point, the timeout has been reached without the structure group being created, thus the test failed
		Assert.Fail("Structure group was not created");
	}

	[Fact]
	public async Task AddPendingStructureGroup_ShouldReturnConflict_WhenFolderAlreadyExists()
	{
		// Arrange
		PendingStructureGroup pendingStructureGroup = new() { Name = "Test" };
		IOptions<NextcloudOptions> nextcloudOptions =
			_scope.ServiceProvider.GetRequiredService<IOptions<NextcloudOptions>>();
		string path = nextcloudOptions.Value.CombineWithStructureBasePath(pendingStructureGroup.Name);
		_webAppFactory.NextcloudServer.SetupGetSingleResource(nextcloudOptions.Value.Username,
			nextcloudOptions.Value.Password, path);

		// Act
		Result result = await _structureGroupsService.AddPendingAsync(pendingStructureGroup);

		// Assert
		result.Status.Should().Be(ResultStatus.Conflict);
	}

	[Fact]
	public async Task AddPendingStructureGroup_ShouldReturnConflict_WhenPendingStructureGroupAlreadyExists()
	{
		// Arrange
		PendingStructureGroup pendingStructureGroup = new() { Name = "Test" };
		IApplicationDbContext dbContext = _scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
		dbContext.PendingStructureGroups.Add(pendingStructureGroup);
		await dbContext.SaveChangesAsync();

		// Act
		Result result = await _structureGroupsService.AddPendingAsync(pendingStructureGroup);

		// Assert
		result.Status.Should().Be(ResultStatus.Conflict);
	}

	[Fact]
	public async Task AddPendingStructureGroup_ShouldReturnConflict_WhenStructureGroupAlreadyExists()
	{
		// Arrange
		PendingStructureGroup pendingStructureGroup = new() { Name = "Test" };
		IApplicationDbContext dbContext = _scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
		dbContext.StructureGroups.Add(pendingStructureGroup.Adapt<StructureGroup>());
		await dbContext.SaveChangesAsync();

		// Act
		Result result = await _structureGroupsService.AddPendingAsync(pendingStructureGroup);

		// Assert
		result.Status.Should().Be(ResultStatus.Conflict);
	}
}