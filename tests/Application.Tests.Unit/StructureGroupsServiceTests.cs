using AutoFixture;
using FluentAssertions;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.StructureGroups;
using Kundenportal.AdminUi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Application.Tests.Unit;

public class StructureGroupsServiceTests
{
    private readonly StructureGroupsService _sut;
    private readonly IApplicationDbContext _dbContext = InMemoryDbContextProvider.GetDbContext();

    private readonly Fixture _fixture = new();
    
    public StructureGroupsServiceTests()
    {
        _sut = new StructureGroupsService(_dbContext);
    }
    
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
}