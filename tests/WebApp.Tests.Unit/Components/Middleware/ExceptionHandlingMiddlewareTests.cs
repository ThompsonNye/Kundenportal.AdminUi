using FluentAssertions;
using Kundenportal.AdminUi.WebApp.Components.Middleware;
using Kundenportal.AdminUi.WebApp.Components.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharedUnitTestLogic;

namespace WebApp.Tests.Unit.Components.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly ExceptionHandlingMiddleware _sut;
    private readonly RequestDelegate _next = Substitute.For<RequestDelegate>();

    private readonly ILogger<ExceptionHandlingMiddleware> _logger =
        Substitute.For<ILogger<ExceptionHandlingMiddleware>>();

    public ExceptionHandlingMiddlewareTests()
    {
        _sut = new ExceptionHandlingMiddleware(_next, _logger);
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetStatusCode500_WhenRequestHasNotStarted()
    {
        // Arrange
        HttpContext context = Substitute.For<HttpContext>();
        HttpResponse response = Substitute.For<HttpResponse>();
        response.Body.Returns(Substitute.For<Stream>());
        context.Response.Returns(response);
        _next.Invoke(context).ThrowsAsync(new Exception());

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        _logger.ReceivedLog<Exception>(LogLevel.Error, "Unhandled error occurred in the application");
        context.Response.StatusCode.Should().Be(500);
    }
    
    [Fact]
    public async Task InvokeAsync_ShouldReturnEarly_WhenRequestHasStarted()
    {
        // Arrange
        HttpContext context = Substitute.For<HttpContext>();
        HttpResponse response = Substitute.For<HttpResponse>();
        response.HasStarted.Returns(true);
        context.Response.Returns(response);
        _next.Invoke(context).ThrowsAsync(new Exception());

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        context.Response.DidNotReceive().StatusCode = 500;
    }
    
    [Fact]
    public async Task InvokeAsync_ShouldNotHandleException_WhenNoExceptionIsThrown()
    {
        // Arrange
        HttpContext context = Substitute.For<HttpContext>();

        // Act
        await _sut.InvokeAsync(context);

        // Assert
        _logger.ReceivedCalls().Should().BeEmpty();
    }
}