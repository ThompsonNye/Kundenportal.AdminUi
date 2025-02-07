using Ardalis.Result;
using FluentAssertions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.StructureGroups;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebApp.Tests.Integration;

public sealed class CreateStructureGroupTests : IClassFixture<WebAppFactory>, IDisposable
{
	private readonly WebAppFactory _webAppFactory;
	private readonly IServiceScope _scope;
	private readonly IStructureGroupsService _structureGroupsService;

	public CreateStructureGroupTests(WebAppFactory webAppFactory)
	{
		_webAppFactory = webAppFactory;
		_scope = webAppFactory.Services.CreateScope();
		_structureGroupsService = _scope.ServiceProvider.GetRequiredService<IStructureGroupsService>();
	}

	[Fact]
	public async Task AddPendingStructureGroup_ShouldResultInStructureGroupBeingCreated_WhenNoConflictExists()
	{
		// Arrange
		PendingStructureGroup pendingStructureGroup = new()
		{
			Name = "Test"
		};
		IOptions<NextcloudOptions> nextcloudOptions = _scope.ServiceProvider.GetRequiredService<IOptions<NextcloudOptions>>();
		string path = nextcloudOptions.Value.CombineWithStructureBasePath(pendingStructureGroup.Name);
		_webAppFactory.NextcloudServer.SetupGetEmptyResources(nextcloudOptions.Value.Username, nextcloudOptions.Value.Password, path);
		_webAppFactory.NextcloudServer.SetupCreateFolder(nextcloudOptions.Value.Username, nextcloudOptions.Value.Password, path);

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

	public void Dispose()
	{
		_scope.Dispose();
	}
}
