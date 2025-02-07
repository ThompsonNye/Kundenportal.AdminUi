using FluentValidation;
using FluentValidation.Results;

namespace Kundenportal.AdminUi.WebApp.Endpoints;

public class ValidationFilter<T> : IEndpointFilter
{
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		object? argToValidate = context.Arguments.FirstOrDefault(x => x is T);

		if (argToValidate is null) return await next.Invoke(context);

		IValidator<T>? validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

		if (validator is null) return await next.Invoke(context);

		ValidationResult? validationResult = await validator.ValidateAsync((T)argToValidate);
		if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

		return await next.Invoke(context);
	}
}
