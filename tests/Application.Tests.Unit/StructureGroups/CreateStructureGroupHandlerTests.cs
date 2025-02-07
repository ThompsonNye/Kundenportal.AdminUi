using AutoFixture;
using FluentAssertions;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Kundenportal.AdminUi.Application.StructureGroups;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharedUnitTestLogic;

namespace Application.Tests.Unit.StructureGroups;

public sealed class CreateStructureGroupHandlerTests
{
    private readonly CreateStructureGroupHandler _sut;
    
    private readonly IOptions<NextcloudOptions> _options = Substitute.For<IOptions<NextcloudOptions>>();
    private readonly INextcloudApi _nextcloudApi = Substitute.For<INextcloudApi>();
    private readonly IApplicationDbContext _dbContext = InMemoryDbContextProvider.GetDbContext();
    private readonly ILogger<CreateStructureGroupHandler> _logger =
        Substitute.For<ILogger<CreateStructureGroupHandler>>();

    private readonly NextcloudOptions _nextcloudOptions;
    private readonly PendingStructureGroupCreated _event;
    
    private readonly Fixture _fixture = new();
   
    public CreateStructureGroupHandlerTests()
    {
        _nextcloudOptions = _fixture.Create<NextcloudOptions>();
        _options.Value.Returns(_nextcloudOptions);

        _event = _fixture.Create<PendingStructureGroupCreated>();

        _sut = new CreateStructureGroupHandler(
            _options,
            _nextcloudApi,
            _dbContext,
            _logger);
    }

    [Fact]
    public async Task Consume_ShouldCreateStructureGroup_WhenEventIsReceived()
    {
        // Arrange
        ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);
        
        // Act
        await _sut.Consume(context);

        // Assert
        string path = $"{_nextcloudOptions.StructureBasePath}/{context.Message.Name}"; 
        
        _dbContext.StructureGroups.Should().ContainEquivalentOf(new
        {
            context.Message.Id,
            context.Message.Name,
            Path = path
        });
        await context.Received().Publish(Arg.Is<StructureGroupCreated>(x => x.Name == context.Message.Name));
        _logger.ReceivedLog(
            LogLevel.Information,
            "Created folder in nextcloud at path {Path}",
            path);
        _logger.ReceivedLog(
            LogLevel.Debug,
            "Saved structure group with {Id} and removed pending structure group with same id in database",
            context.Message.Id);
    }

    [Fact]
    public async Task Consume_ShouldRemovePendingStructureGroupFromDb_WhenStructureGroupIsCreated()
    {
        // Arrange
        ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);
        _dbContext.PendingStructureGroups.Add(context.Message.Adapt<PendingStructureGroup>());
        await _dbContext.SaveChangesAsync();
        
        // Act
        await _sut.Consume(context);

        // Assert
        _dbContext.PendingStructureGroups.Should().NotContain(x => x.Id == context.Message.Id && x.Name == context.Message.Name);
        _dbContext.StructureGroups.Should().Contain(x => x.Id == context.Message.Id && x.Name == context.Message.Name);
    }

    [Fact]
    public async Task Consume_ShouldRethrowException_WhenNextcloudApiThrowsException()
    {
        // Arrange
        ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);
        
        string path = $"{_nextcloudOptions.StructureBasePath}/{context.Message.Name}";
        _nextcloudApi.CreateFolderAsync(path, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception());

        // Act
        Func<Task> action = () => _sut.Consume(context);

        // Assert
        await action.Should().ThrowExactlyAsync<Exception>();
        _logger.ReceivedLog<Exception>(
            LogLevel.Error,
            "Failed to create a folder for the structure group with name {Name} in Nextcloud",
            context.Message.Name);
    }

    [Fact]
    public async Task Consume_ShouldExitEarly_WhenStructureGroupAlreadyExists()
    {
        // Arrange
        ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

        _dbContext.StructureGroups.Add(context.Message.Adapt<StructureGroup>());
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.Consume(context);

        // Assert
        _logger.ReceivedCalls().Should().BeEmpty();
        await _nextcloudApi.DidNotReceive().CreateFolderAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}