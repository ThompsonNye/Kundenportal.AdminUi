using AutoFixture;
using Kundenportal.AdminUi.Application.Hubs;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.StructureGroups;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharedUnitTestLogic;

namespace Application.Tests.Unit.StructureGroups;

public sealed class NotifyStructureGroupOverviewOfNewStructureGroupHandlerTests
{
    private readonly NotifyStructureGroupOverviewOfNewStructureGroupHandler _sut;
    private readonly IHubContext<StructureGroupHub> _hubContext = Substitute.For<IHubContext<StructureGroupHub>>();
    private readonly ILogger<NotifyStructureGroupOverviewOfNewStructureGroupHandler> _logger =
        Substitute.For<ILogger<NotifyStructureGroupOverviewOfNewStructureGroupHandler>>();

    private readonly Fixture _fixture = new();

    private readonly StructureGroupCreated _event;
    
    public NotifyStructureGroupOverviewOfNewStructureGroupHandlerTests()
    {
        _event = _fixture.Create<StructureGroupCreated>();
        
        _sut = new NotifyStructureGroupOverviewOfNewStructureGroupHandler(_hubContext, _logger);
    }

    [Fact]
    public async Task Consume_ShouldSendMessage_WhenCalled()
    {
        // Arrange
        IHubClients hubClients = Substitute.For<IHubClients>();
        IClientProxy clientProxy = Substitute.For<IClientProxy>();

        hubClients.All.Returns(clientProxy);
        _hubContext.Clients.Returns(hubClients);
        
        ConsumeContext<StructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

        // Act
        await _sut.Consume(context);

        // Asserts
        await clientProxy.SendCoreAsync(
            StructureGroupHub.NewStructureGroupMethod, 
            Arg.Is<object[]>(x => ((StructureGroup)x[0]).Id == context.Message.Id),
            context.CancellationToken);
    }

    [Fact]
    public async Task Consume_ShouldLogError_WhenMessageIsFailedToSend()
    {
        // Arrange
        _hubContext.Clients.Throws<Exception>();
        
        ConsumeContext<StructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

        // Act
        await _sut.Consume(context);

        // Assert
        _logger.ReceivedLog<Exception>(
            LogLevel.Error,
            "Failed to send message to the structure group hub");
    }
}