using AutoFixture;
using FluentAssertions;
using Kundenportal.AdminUi.Application;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using NSubstitute;

namespace Application.Tests.Unit;

public class StructureGroupsServiceTests
{
    private readonly StructureGroupsService _sut;
    private readonly IApplicationDbContext _dbContext = Substitute.For<IApplicationDbContext>();

    private readonly Fixture _fixture = new();
    
    public StructureGroupsServiceTests()
    {
        _sut = new StructureGroupsService(_dbContext);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyArray_WhenNoStructureGroupsExist()
    {
        // Arrange
        StructureGroup[] structureGroups = [];
        _dbContext.StructureGroups.Returns(structureGroups.AsQueryable());

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
        _dbContext.StructureGroups.Returns(structureGroups.AsQueryable());

        // Act
        IEnumerable<StructureGroup> result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEquivalentTo(structureGroups);
    }
}