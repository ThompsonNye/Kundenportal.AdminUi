using FluentValidation;

namespace Kundenportal.AdminUi.WebApp.Endpoints;

public class ValidationFilter<T> : IEndpointFilter
{
	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
	{
		var argToValidate = context.Arguments.FirstOrDefault(x => x is T);

		if (argToValidate is null) return await next.Invoke(context);

		var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

		if (validator is null) return await next.Invoke(context);

		var validationResult = await validator.ValidateAsync((T)argToValidate);
		if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

		return await next.Invoke(context);
	}
}
