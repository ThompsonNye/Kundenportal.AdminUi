﻿using Ardalis.Result;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public interface IStructureGroupsService
{
	Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default);

	Task<IEnumerable<PendingStructureGroup>> GetPendingAsync(CancellationToken cancellationToken = default);

	Task<Result> AddPendingAsync(PendingStructureGroup pendingStructureGroup,
		CancellationToken cancellationToken = default);
}

public sealed class StructureGroupsService(
	IApplicationDbContext dbContext,
	INextcloudApi nextcloud,
	IOptions<NextcloudOptions> nextcloudOptions,
	IPublishEndpoint publishEndpoint,
	ILogger<StructureGroupsService> logger,
	ActivitySource activitySource)
	: IStructureGroupsService
{
	private readonly ActivitySource _activitySource = activitySource;
	private readonly IApplicationDbContext _dbContext = dbContext;
	private readonly ILogger<StructureGroupsService> _logger = logger;
	private readonly INextcloudApi _nextcloud = nextcloud;
	private readonly IOptions<NextcloudOptions> _nextcloudOptions = nextcloudOptions;
	private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public async Task<IEnumerable<StructureGroup>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _semaphore.WaitAsync(2000, cancellationToken);
			StructureGroup[] structureGroups = await _dbContext.StructureGroups
				.OrderBy(x => x.Name)
				.ToArrayAsync(cancellationToken);
			return structureGroups;
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<IEnumerable<PendingStructureGroup>> GetPendingAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			await _semaphore.WaitAsync(2000, cancellationToken);
			PendingStructureGroup[] pendingStructureGroups =
				await _dbContext.PendingStructureGroups.ToArrayAsync(cancellationToken);
			return pendingStructureGroups;
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<Result> AddPendingAsync(PendingStructureGroup pendingStructureGroup,
		CancellationToken cancellationToken = default)
	{
		using Activity? activity = _activitySource.StartActivity("AddPendingStructureGroup");

		bool structureGroupExists = await DoesStructureGroupExistAsync(pendingStructureGroup.Name, cancellationToken);
		if (structureGroupExists)
		{
			return Result.Conflict();
		}

		_logger.LogDebug("Creating pending structure group with id {Id} and name {Name}", pendingStructureGroup.Id,
			pendingStructureGroup.Name);

		_dbContext.PendingStructureGroups.Add(pendingStructureGroup);

		PendingStructureGroupCreated pendingStructureGroupCreated =
			pendingStructureGroup.Adapt<PendingStructureGroupCreated>();
		await _publishEndpoint.Publish(pendingStructureGroupCreated, cancellationToken);

		await _dbContext.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("Pending structure group created");

		return Result.Success();
	}

	private async Task<bool> DoesStructureGroupExistAsync(string name, CancellationToken cancellationToken = default)
	{
		using Activity? activity = _activitySource.StartActivity();

		bool pendingGroupExists =
			await DoesAPendingStructureGroupWithThatNameAlreadyExistAsync(name, cancellationToken);

		if (pendingGroupExists)
		{
			return true;
		}
		
		bool structureGroupExists =
			await DoesAStructureGroupWithThatNameAlreadyExistAsync(name, cancellationToken);

		if (structureGroupExists)
		{
			return true;
		}

		bool existsInNextcloud = await DoesStructureGroupFolderExistInNextcloudAsync(name, cancellationToken);
		return existsInNextcloud;
	}

	private async Task<bool> DoesStructureGroupFolderExistInNextcloudAsync(string name,
		CancellationToken cancellationToken)
	{
		string path = _nextcloudOptions.Value.CombineWithStructureBasePath(name);
		return await _nextcloud.DoesFolderExistAsync(path, cancellationToken);
	}

	private async Task<bool> DoesAPendingStructureGroupWithThatNameAlreadyExistAsync(string name,
		CancellationToken cancellationToken = default)
	{
		bool exists = await _dbContext
			.PendingStructureGroups
			.AnyAsync(x => x.Name == name, cancellationToken);

		return exists;
	}
	
	private async Task<bool> DoesAStructureGroupWithThatNameAlreadyExistAsync(string name,
		CancellationToken cancellationToken = default)
	{
		bool exists = await _dbContext
			.StructureGroups
			.AnyAsync(x => x.Name == name, cancellationToken);

		return exists;
	}
}
