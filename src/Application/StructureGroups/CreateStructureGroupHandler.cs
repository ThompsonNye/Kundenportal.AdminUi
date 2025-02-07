using System.Net;
using System.Web;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using Kundenportal.AdminUi.Application.Options;
using Kundenportal.AdminUi.Application.Services;
using MassTransit;
using MassTransit.Configuration;
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
        await CreateFolderInNextcloudAsync(context);

        await UpdateDbAsync(context);
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
            _dbContext.StructureGroups.Add(new StructureGroup
            {
                Id = context.Message.Id,
                Name = context.Message.Name
            });

            PendingStructureGroup? pendingStructureGroup = await _dbContext.PendingStructureGroups.FindAsync([ context.Message.Id ], context.CancellationToken);
            if (pendingStructureGroup is not null)
            {
                _dbContext.PendingStructureGroups.Remove(pendingStructureGroup);
            }
            
            await context.Publish(new StructureGroupCreated
            {
                Id = context.Message.Id,
                Name = context.Message.Name
            });
            
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

public class StructureGroupCreated
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }
}
