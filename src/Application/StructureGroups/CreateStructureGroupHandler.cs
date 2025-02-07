using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Models.Exceptions;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using Mapster;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kundenportal.AdminUi.Application.StructureGroups;

public sealed class CreateStructureGroupHandler(
    IOptions<NextcloudOptions> nextcloudOptions,
    INextcloudApi nextcloudApi,
    IApplicationDbContext dbContext,
    ILogger<CreateStructureGroupHandler> logger)
    : IConsumer<PendingStructureGroupCreated>
{
    private readonly IOptions<NextcloudOptions> _nextcloudOptions = nextcloudOptions;
    private readonly INextcloudApi _nextcloudApi = nextcloudApi;
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<CreateStructureGroupHandler> _logger = logger;

    public async Task Consume(ConsumeContext<PendingStructureGroupCreated> context)
    {
        bool existsInDb = await StructureGroupExistsInDbAsync(context); 
        if (existsInDb)
        {
            _logger.LogDebug("Structure group {Name} exists in db, skipping", context.Message.Name);
            return;
        }

        CreateStructureGroupFolderResult result = await CreateFolderInNextcloudAsync(context);
        await UpdateDbAsync(context, result);
    }

    private async Task<bool> StructureGroupExistsInDbAsync(ConsumeContext<PendingStructureGroupCreated> context)
    {
        StructureGroup? existingStructureGroup = await _dbContext.StructureGroups.FindAsync(
            [context.Message.Id], context.CancellationToken);
        return existingStructureGroup is not null;
    }

    private async Task<CreateStructureGroupFolderResult> CreateFolderInNextcloudAsync(ConsumeContext<PendingStructureGroupCreated> context)
    {
        try
        {
            string path = _nextcloudOptions.Value.CombineWithStructureBasePath(context.Message.Name);

            await _nextcloudApi.CreateFolderAsync(path, context.CancellationToken);

            _logger.LogInformation("Created folder in nextcloud at path {Path}", path);

            return new CreateStructureGroupFolderResult
            {
                Path = path
            };
        }
        catch (ApplicationException ex)
            when (ex is NextcloudRequestException or NextcloudFolderExistsException)
        {
            _logger.LogError("Failed to create a folder for the structure group with name {Name} in Nextcloud due to a Nextcloud related issue: {Message}", context.Message.Name, ex.Message);
            
            // Rethrowing the exception here so the message is retried later
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create a folder for the structure group with name {Name} in Nextcloud", context.Message.Name);
            
            // Rethrowing the exception here so the message is retried later
            throw;
        }
    }

    private async Task UpdateDbAsync(
        ConsumeContext<PendingStructureGroupCreated> context,
        CreateStructureGroupFolderResult createStructureGroupFolderResult)
    {
        try
        {
            StructureGroup structureGroup = context.Message.Adapt<StructureGroup>();
            structureGroup.Path = createStructureGroupFolderResult.Path;
            _dbContext.StructureGroups.Add(structureGroup);

            PendingStructureGroup? pendingStructureGroup = await _dbContext.PendingStructureGroups.FindAsync(
                    [ context.Message.Id ], context.CancellationToken);
            if (pendingStructureGroup is not null)
            {
                _dbContext.PendingStructureGroups.Remove(pendingStructureGroup);
            }

            StructureGroupCreated structureGroupCreated = context.Message.Adapt<StructureGroupCreated>();
            await context.Publish(structureGroupCreated);
            
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            
            _logger.LogDebug("Saved structure group with {Id} and removed pending structure group with same id in database", context.Message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save the structure group in the db");

            await DeleteFolderAsync(context);

            throw;
        }
    }

    private async Task DeleteFolderAsync(ConsumeContext<PendingStructureGroupCreated> context)
    {
        string path = _nextcloudOptions.Value.CombineWithStructureBasePath(context.Message.Name);
        try
        {
            await _nextcloudApi.DeleteFolderAsync(path, context.CancellationToken);
        }
        catch (NextcloudRequestException ex)
        {
            _logger.LogWarning("Got Nextcloud request error while deleting folder at {Path}: {Message}", path, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete folder at {Path}", path);
        }
    }
}

public sealed class CreateStructureGroupFolderResult
{
    public required string Path { get; init; }
}
