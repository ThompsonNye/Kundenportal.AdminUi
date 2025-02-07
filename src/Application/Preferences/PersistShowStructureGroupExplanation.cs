using FluentValidation;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kundenportal.AdminUi.Application.Preferences;

public class PersistShowStructureGroupExplanation
{
    public required Guid UserId { get; set; }

    public required bool HideExplanation { get; set; }
}

public class PersistShowStructureGroupExplanationValidator : AbstractValidator<PersistShowStructureGroupExplanation>
{
    public PersistShowStructureGroupExplanationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(_ => default);
    }
}

public class PersistShowStructureGroupExplanationConsumer(
    ILogger<PersistShowStructureGroupExplanationConsumer> logger,
    IApplicationDbContext dbContext)
    : IConsumer<PersistShowStructureGroupExplanation>
{
    private readonly ILogger<PersistShowStructureGroupExplanationConsumer> _logger = logger;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task Consume(ConsumeContext<PersistShowStructureGroupExplanation> context)
    {
        _logger.LogDebug("Persisting show status for structure group explanation for user {UserId}",
            context.Message.UserId);

        UserPreferences? existingPreferences = await _dbContext.UserPreferences.SingleOrDefaultAsync(
            x => x.UserId == context.Message.UserId,
            context.CancellationToken);

        if (existingPreferences is null)
        {
            await AddPreferences(context);
            return;
        }

        await UpdatePreferences(existingPreferences, context);
    }

    private async Task UpdatePreferences(UserPreferences preferences,
        ConsumeContext<PersistShowStructureGroupExplanation> context)
    {
        preferences.HideStructureGroupExplanation = context.Message.HideExplanation;
        _dbContext.UserPreferences.Update(preferences);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogDebug("Persisted show status for structure group explanation for user {UserId}",
            context.Message.UserId);
    }

    private async Task AddPreferences(ConsumeContext<PersistShowStructureGroupExplanation> context)
    {
        UserPreferences preferences = new()
        {
            UserId = context.Message.UserId,
            HideStructureGroupExplanation = context.Message.HideExplanation
        };
        await _dbContext.UserPreferences.AddAsync(preferences, context.CancellationToken);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogDebug("Persisted show status for structure group explanation for user {UserId}",
            context.Message.UserId);
    }
}