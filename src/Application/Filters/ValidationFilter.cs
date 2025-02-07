using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Kundenportal.AdminUi.Application.Filters;

public class ValidationFilter<T>(
    IEnumerable<IValidator<T>> validators) 
    : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly IEnumerable<IValidator<T>> _validators = validators;

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        Task<ValidationResult>[] validationResultTasks = _validators
            .Select(x => x.ValidateAsync(context.Message, context.CancellationToken))
            .ToArray();

        if (validationResultTasks.Length == 0)
        {
            await next.Send(context);
            return;
        }

        await Task.WhenAll(validationResultTasks);

        ValidationFailure[] validationFailures = validationResultTasks
            .Select(x => x.Result)
            .Where(x => !x.IsValid)
            .SelectMany(x => x.Errors)
            .ToArray();

        if (validationFailures.Length > 0)
        {
            throw new ValidationException(validationFailures);
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateMessageScope($"{typeof(T).Name}-validation");
    }
}