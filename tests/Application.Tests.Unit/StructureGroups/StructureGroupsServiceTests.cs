using System.Diagnostics;
using AutoFixture;
using FluentAssertions;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Kundenportal.AdminUi.Application.StructureGroups;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharedUnitTestLogic;

namespace Application.Tests.Unit.StructureGroups;

public class StructureGroupsServiceTests
{
	private readonly IApplicationDbContext _dbContext = InMemoryDbContextProvider.GetDbContext();

	private readonly Fixture _fixture = new();
	private readonly ILogger<StructureGroupsService> _logger = Substitute.For<ILogger<StructureGroupsService>>();
	private readonly INextcloudApi _nextcloudApi = Substitute.For<INextcloudApi>();
	private readonly IOptions<NextcloudOptions> _nextcloudOptions = Substitute.For<IOptions<NextcloudOptions>>();
	private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
	private readonly StructureGroupsService _sut;

	public StructureGroupsServiceTests()
	{
		_sut = new StructureGroupsService(
			_dbContext,
			_nextcloudApi,
			_nextcloudOptions,
			_publishEndpoint,
			_logger,
			new ActivitySource(""));
	}

	#region GetAllAsync

	[Fact]
	public async Task GetAllAsync_ShouldReturnEmptyArray_WhenNoStructureGroupsExist()
	{
		// Arrange

		// Act
		var result = await _sut.GetAllAsync();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAllAsync_ShouldReturnItems_WhenItemsExist()
	{
		// Arrange
		var structureGroups = _fixture.CreateMany<StructureGroup>()
			.ToArray();
		_dbContext.StructureGroups.AddRange(structureGroups);
		await _dbContext.SaveChangesAsync();

		// Act
		var result = await _sut.GetAllAsync();

		// Assert
		result.Should().BeEquivalentTo(structureGroups);
	}

	#endregion

	#region GetPendingAsync

	[Fact]
	public async Task GetPendingAsync_ShouldReturnEmptyArray_WhenNoStructureGroupsExist()
	{
		// Arrange

		// Act
		var result = await _sut.GetPendingAsync();

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetPendingAsync_ShouldReturnItems_WhenItemsExist()
	{
		// Arrange
		var structureGroups = _fixture.CreateMany<PendingStructureGroup>()
			.ToArray();
		_dbContext.PendingStructureGroups.AddRange(structureGroups);
		await _dbContext.SaveChangesAsync();

		// Act
		var result = await _sut.GetPendingAsync();

		// Assert
		result.Should().BeEquivalentTo(structureGroups);
	}

	#endregion

	#region AddPendingAsync

	[Fact]
	public async Task AddPendingAsync_ShouldAddPendingStructureGroup_WhenCalled()
	{
		// Arrange
		var pendingStructureGroup = _fixture.Create<PendingStructureGroup>();

		// Act
		await _sut.AddPendingAsync(pendingStructureGroup);

		// Assert
		_dbContext.PendingStructureGroups
			.Should()
			.ContainSingle(x => x.Id == pendingStructureGroup.Id);
	}

	[Fact]
	public async Task AddPendingAsync_ShouldLogMessages_WhenCalled()
	{
		// Arrange
		var pendingStructureGroup = _fixture.Create<PendingStructureGroup>();

		// Act
		await _sut.AddPendingAsync(pendingStructureGroup);

		// Assert
		_logger.ReceivedLog(
			LogLevel.Debug,
			"Creating pending structure group with id {Id} and name {Name}",
			pendingStructureGroup.Id,
			pendingStructureGroup.Name);
		_logger.Received().LogInformation("Pending structure group created");
	}

	#endregion

	#region DoesStructureGroupExistAsync

	[Fact]
	public async Task DoesStructureGroupExistAsync_ShouldReturnTrue_WhenAPendingStructureGroupsExistsInDb()
	{
		// Arrange
		const string path = "path";
		_dbContext.PendingStructureGroups.Add(new PendingStructureGroup
		{
			Name = path
		});
		await _dbContext.SaveChangesAsync();

		// Act
		var result = await _sut.DoesStructureGroupExistAsync(path);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task
		DoesStructureGroupExistAsync_ShouldReturnTrue_WhenNextcloudFolderExistsInRootFolder()
	{
		// Arrange
		const string structureGroupName = "path";

		var nextcloudOptions = new NextcloudOptions
		{
			StructureBasePath = "/"
		};
		_nextcloudOptions.Value.Returns(nextcloudOptions);

		var path = nextcloudOptions.CombineWithStructureBasePath(structureGroupName);
		_nextcloudApi.DoesFolderExistAsync(path).Returns(true);

		// Act
		var result = await _sut.DoesStructureGroupExistAsync(structureGroupName);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task
		DoesStructureGroupExistAsync_ShouldReturnTrue_WhenNextcloudFolderExistsSubFolder()
	{
		// Arrange
		const string structureGroupName = "path";

		var nextcloudOptions = new NextcloudOptions
		{
			StructureBasePath = "/Structures"
		};
		_nextcloudOptions.Value.Returns(nextcloudOptions);

		var path = nextcloudOptions.CombineWithStructureBasePath(structureGroupName);
		_nextcloudApi.DoesFolderExistAsync(path).Returns(true);

		// Act
		var result = await _sut.DoesStructureGroupExistAsync(structureGroupName);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task
		DoesStructureGroupExistAsync_ShouldReturnFalse_WhenNextcloudDoesNotExist()
	{
		// Arrange
		const string structureGroupName = "path";

		NextcloudOptions nextcloudOptions = new();
		_nextcloudOptions.Value.Returns(nextcloudOptions);

		var path = nextcloudOptions.CombineWithStructureBasePath(structureGroupName);
		_nextcloudApi.DoesFolderExistAsync(path).Returns(false);

		// Act
		var result = await _sut.DoesStructureGroupExistAsync(path);

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public async Task DoesStructureGroupExistAsync_ShouldReturnFalse_WhenNextcloudThrowsException()
	{
		// Arrange
		const string structureGroupName = "path";

		NextcloudOptions nextcloudOptions = new();
		_nextcloudOptions.Value.Returns(nextcloudOptions);

		_nextcloudApi.DoesFolderExistAsync(Arg.Any<string>()).ThrowsAsync<Exception>();

		// Act
		Func<Task> action = () => _sut.DoesStructureGroupExistAsync(structureGroupName);

		// Assert
		await action.Should().ThrowExactlyAsync<Exception>();
	}

	#endregion
}
