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

namespace Application.Tests.Unit;

public class StructureGroupsServiceTests
{
    private readonly StructureGroupsService _sut;
    private readonly IApplicationDbContext _dbContext = InMemoryDbContextProvider.GetDbContext();
    private readonly INextcloudApi _nextcloudApi = Substitute.For<INextcloudApi>();
    private readonly IOptions<NextcloudOptions> _nextcloudOptions = Substitute.For<IOptions<NextcloudOptions>>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly ILogger<StructureGroupsService> _logger = Substitute.For<ILogger<StructureGroupsService>>();

    private readonly Fixture _fixture = new();

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
        IEnumerable<StructureGroup> result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnItems_WhenItemsExist()
    {
        // Arrange
        StructureGroup[] structureGroups = _fixture.CreateMany<StructureGroup>()
            .ToArray();
        _dbContext.StructureGroups.AddRange(structureGroups);
        await _dbContext.SaveChangesAsync();

        // Act
        IEnumerable<StructureGroup> result = await _sut.GetAllAsync();

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
        IEnumerable<PendingStructureGroup> result = await _sut.GetPendingAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnItems_WhenItemsExist()
    {
        // Arrange
        PendingStructureGroup[] structureGroups = _fixture.CreateMany<PendingStructureGroup>()
            .ToArray();
        _dbContext.PendingStructureGroups.AddRange(structureGroups);
        await _dbContext.SaveChangesAsync();

        // Act
        IEnumerable<PendingStructureGroup> result = await _sut.GetPendingAsync();

        // Assert
        result.Should().BeEquivalentTo(structureGroups);
    }

    #endregion

    #region AddPendingAsync

    [Fact]
    public async Task AddPendingAsync_ShouldAddPendingStructureGroup_WhenCalled()
    {
        // Arrange
        PendingStructureGroup pendingStructureGroup = _fixture.Create<PendingStructureGroup>();

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
        PendingStructureGroup pendingStructureGroup = _fixture.Create<PendingStructureGroup>();

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
        bool result = await _sut.DoesStructureGroupExistAsync(path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task
        DoesStructureGroupExistAsync_ShouldReturnTrue_WhenNextcloudReturnsFolderDetailsForStructureInRootFolder()
    {
        // Arrange
        const string path = "path";

        NextcloudOptions nextcloudOptions = new NextcloudOptions();
        _nextcloudOptions.Value.Returns(nextcloudOptions);

        NextcloudFolder nextcloudFolder = _fixture.Create<NextcloudFolder>();
        _nextcloudApi.GetFolderDetailsAsync($"{nextcloudOptions.StructureBasePath}/{path}").Returns(nextcloudFolder);

        // Act
        bool result = await _sut.DoesStructureGroupExistAsync(path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task
        DoesStructureGroupExistAsync_ShouldReturnTrue_WhenNextcloudReturnsFolderDetailsForStructureInSubFolder()
    {
        // Arrange
        const string path = "path";

        NextcloudOptions nextcloudOptions = new NextcloudOptions
        {
            StructureBasePath = "/Structures"
        };
        _nextcloudOptions.Value.Returns(nextcloudOptions);

        NextcloudFolder nextcloudFolder = _fixture.Create<NextcloudFolder>();
        _nextcloudApi.GetFolderDetailsAsync($"{nextcloudOptions.StructureBasePath}/{path}").Returns(nextcloudFolder);

        // Act
        bool result = await _sut.DoesStructureGroupExistAsync(path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DoesStructureGroupExistAsync_ShouldReturnFalse_WhenNextcloudThrowsException()
    {
        // Arrange
        const string path = "path";
        _nextcloudApi.GetFolderDetailsAsync(path).ThrowsAsync<Exception>();

        // Act
        bool result = await _sut.DoesStructureGroupExistAsync(path);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}