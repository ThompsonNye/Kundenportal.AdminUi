using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
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
        if (await StructureGroupExistsAsync(context))
        {
            return;
        }
        
        await CreateFolderInNextcloudAsync(context);

        await UpdateDbAsync(context);
    }

    private async Task<bool> StructureGroupExistsAsync(ConsumeContext<PendingStructureGroupCreated> context)
    {
        try
        {
            StructureGroup? existingStructureGroup = await _dbContext.StructureGroups.FindAsync(
                [context.Message.Id], context.CancellationToken);
            return existingStructureGroup is not null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check whether the structure group already exists");
            
            // Rethrowing the exception here so MassTransit automatically retries later
            throw;
        }
    }

    private async Task CreateFolderInNextcloudAsync(ConsumeContext<PendingStructureGroupCreated> context)
    {
        try
        {
            // No need to url encode the folder name since only valid values
            // (e.g. without / in the name) can be submitted in the ui
            string path = $"{_nextcloudOptions.Value.StructureBasePath}/{context.Message.Name}";

            await _nextcloudApi.CreateFolderAsync(path, context.CancellationToken);

            _logger.LogInformation("Created folder in nextcloud at path {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create a folder for the structure group with name {Name} in Nextcloud", context.Message.Name);
            
            // Rethrowing the exception here so MassTransit automatically retries later
            throw;
        }
    }

    private async Task UpdateDbAsync(ConsumeContext<PendingStructureGroupCreated> context)
    {
        try
        {
            StructureGroup structureGroup = context.Message.Adapt<StructureGroup>();
            _dbContext.StructureGroups.Add(structureGroup);

            PendingStructureGroup? pendingStructureGroup = await _dbContext.PendingStructureGroups.FindAsync([ context.Message.Id ], context.CancellationToken);
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
            
            // TODO Delete folder in Nextcloud?
        }
    }
}
