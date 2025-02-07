using FluentValidation;
using Kundenportal.AdminUi.Application.Abstractions;
using Kundenportal.AdminUi.Application.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Kundenportal.AdminUi.Application.Preferences;

public class GetUserPreferences
{
    public required Guid UserId { get; set; }
}

public class GetUserPreferencesValidator : AbstractValidator<GetUserPreferences>
{
    public GetUserPreferencesValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(_ => default);
    }
}

public class GetUserPreferencesConsumer(IApplicationDbContext dbContext)
    : IConsumer<GetUserPreferences>
{
    public async Task Consume(ConsumeContext<GetUserPreferences> context)
    {
        UserPreferences? userPreferences = await dbContext.UserPreferences
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.UserId == context.Message.UserId,
                context.CancellationToken);

        if (userPreferences is not null)
        {
            await context.RespondAsync(userPreferences);
            return;
        }

        UserPreferencesNotFound notFound = new()
        {
            UserId = context.Message.UserId
        };
        await context.RespondAsync(notFound);
    }
}

public class UserPreferencesNotFound
{
    public required Guid UserId { get; set; }
}