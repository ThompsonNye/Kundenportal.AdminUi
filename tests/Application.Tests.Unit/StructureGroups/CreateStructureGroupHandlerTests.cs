using AutoFixture;
using FluentAssertions;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Models.Exceptions;
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
	private readonly IApplicationDbContext _dbContext = InMemoryDbContextProvider.GetDbContext();
	private readonly PendingStructureGroupCreated _event;

	private readonly Fixture _fixture = new();

	private readonly ILogger<CreateStructureGroupHandler> _logger =
		Substitute.For<ILogger<CreateStructureGroupHandler>>();

	private readonly INextcloudApi _nextcloudApi = Substitute.For<INextcloudApi>();

	private readonly NextcloudOptions _nextcloudOptions;

	private readonly IOptions<NextcloudOptions> _options = Substitute.For<IOptions<NextcloudOptions>>();
	private readonly CreateStructureGroupHandler _sut;

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
		_dbContext.PendingStructureGroups.Should()
			.NotContain(x => x.Id == context.Message.Id && x.Name == context.Message.Name);
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
		_logger.ReceivedLog(LogLevel.Debug, "Structure group {Name} exists in db, skipping", context.Message.Name);
		await _nextcloudApi.DidNotReceive().CreateFolderAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	private (CreateStructureGroupHandler, IApplicationDbContext) GetSutWithMockedDbContext()
	{
		IApplicationDbContext? dbContext = Substitute.For<IApplicationDbContext>();
		CreateStructureGroupHandler sut = new(_options, _nextcloudApi, dbContext, _logger);
		return (sut, dbContext);
	}

	[Fact]
	public async Task Consume_ShouldRethrowException_WhenExceptionIsThrownWhenCheckingIfStructureGroupExists()
	{
		// Arrange
		(CreateStructureGroupHandler sut, IApplicationDbContext dbContext) = GetSutWithMockedDbContext();
		dbContext.StructureGroups.Throws<Exception>();

		ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

		// Act
		Func<Task> action = () => sut.Consume(context);

		// Assert
		await action.Should().ThrowExactlyAsync<Exception>();
	}

	[Fact]
	public async Task Consume_ShouldRethrow_WhenNextcloudRequestFails()
	{
		// Arrange
		ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

		string path = _nextcloudOptions.CombineWithStructureBasePath(context.Message.Name);
		NextcloudRequestException exception = new(-1);
		_nextcloudApi.CreateFolderAsync(path, context.CancellationToken).ThrowsAsync(exception);

		// Act
		Func<Task> action = () => _sut.Consume(context);

		// Assert
		(await action.Should().ThrowExactlyAsync<NextcloudRequestException>())
			.And.StatusCode.Should().Be(exception.StatusCode);
		_logger.ReceivedLog(LogLevel.Error,
			"Failed to create a folder for the structure group with name {Name} in Nextcloud due to a Nextcloud related issue: {Message}",
			context.Message.Name, exception.Message);
	}

	[Fact]
	public async Task Consume_ShouldRethrow_WhenFolderInNextcloudAlreadyExists()
	{
		// Arrange
		ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

		string path = _nextcloudOptions.CombineWithStructureBasePath(context.Message.Name);
		NextcloudFolderExistsException exception = new(path);
		_nextcloudApi.CreateFolderAsync(path, context.CancellationToken).ThrowsAsync(exception);

		// Act
		Func<Task> action = () => _sut.Consume(context);

		// Assert
		(await action.Should().ThrowExactlyAsync<NextcloudFolderExistsException>())
			.And.Should().Be(exception);
		_logger.ReceivedLog(LogLevel.Error,
			"Failed to create a folder for the structure group with name {Name} in Nextcloud due to a Nextcloud related issue: {Message}",
			context.Message.Name, exception.Message);
	}

	[Fact]
	public async Task Consume_ShouldDeleteFolderAndRethrow_WhenSaveInDbFails()
	{
		// Arrange
		ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

		string path = _nextcloudOptions.CombineWithStructureBasePath(context.Message.Name);
		_nextcloudApi.CreateFolderAsync(path, context.CancellationToken)
			.Returns(Task.CompletedTask)
			.AndDoes(_ =>
			{
				StructureGroup structureGroup = context.Message.Adapt<StructureGroup>();
				_dbContext.StructureGroups.Add(structureGroup);
				_dbContext.SaveChangesAsync().GetAwaiter().GetResult();
			});

		// Act
		Func<Task> action = () => _sut.Consume(context);

		// Assert
		await action.Should().ThrowAsync<Exception>();
		_logger.ReceivedLog<Exception>(LogLevel.Error, "Failed to save the structure group in the db");
		await _nextcloudApi.Received().DeleteFolderAsync(path, context.CancellationToken);
	}

	[Fact]
	public async Task Consume_ShouldLogWarning_WhenDeletingFolderRequestFails()
	{
		// Arrange
		ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

		string path = _nextcloudOptions.CombineWithStructureBasePath(context.Message.Name);
		_nextcloudApi.CreateFolderAsync(path, context.CancellationToken)
			.Returns(Task.CompletedTask)
			.AndDoes(_ =>
			{
				StructureGroup structureGroup = context.Message.Adapt<StructureGroup>();
				_dbContext.StructureGroups.Add(structureGroup);
				_dbContext.SaveChangesAsync().GetAwaiter().GetResult();
			});
		NextcloudRequestException? exception = _fixture.Create<NextcloudRequestException>();
		_nextcloudApi.DeleteFolderAsync(path, context.CancellationToken).ThrowsAsync(exception);

		// Act
		try
		{
			await _sut.Consume(context);
		}
		catch
		{
			// Ignored
		}

		// Assert
		_logger.ReceivedLog<Exception>(LogLevel.Warning,
			"Got Nextcloud request error while deleting folder at {Path}: {Message}", path, exception.Message);
	}

	[Fact]
	public async Task Consume_ShouldLogWarning_WhenDeletingFolderThrowsException()
	{
		// Arrange
		ConsumeContext<PendingStructureGroupCreated> context = ConsumeContextProvider.GetMockedContext(_event);

		string path = _nextcloudOptions.CombineWithStructureBasePath(context.Message.Name);
		_nextcloudApi.CreateFolderAsync(path, context.CancellationToken)
			.Returns(Task.CompletedTask)
			.AndDoes(_ =>
			{
				StructureGroup structureGroup = context.Message.Adapt<StructureGroup>();
				_dbContext.StructureGroups.Add(structureGroup);
				_dbContext.SaveChangesAsync().GetAwaiter().GetResult();
			});
		_nextcloudApi.DeleteFolderAsync(path, context.CancellationToken).ThrowsAsync<Exception>();

		// Act
		try
		{
			await _sut.Consume(context);
		}
		catch
		{
			// Ignored
		}

		// Assert
		_logger.ReceivedLog<Exception>(LogLevel.Warning, "Failed to delete folder at {Path}", path);
	}
}
