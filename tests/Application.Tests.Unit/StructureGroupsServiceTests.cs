using AutoFixture;
using FluentAssertions;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Services;
using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Application.Tests.Unit;

public class StructureGroupsServiceTests
{
    private readonly StructureGroupsService _sut;
    private readonly IApplicationDbContext _dbContext = InMemoryDbContextProvider.GetDbContext();
    private readonly INextcloudApi _nextcloudApi = Substitute.For<INextcloudApi>();

    private readonly Fixture _fixture = new();
    
    public StructureGroupsServiceTests()
    {
        _sut = new StructureGroupsService(_dbContext, _nextcloudApi);
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
    public async Task DoesStructureGroupExistAsync_ShouldReturnTrue_WhenNextcloudReturnsFolderDetails()
    {
        // Arrange
        const string path = "path";
        NextcloudFolder nextcloudFolder = _fixture.Create<NextcloudFolder>();
        _nextcloudApi.GetFolderDetailsAsync(path).Returns(nextcloudFolder);

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