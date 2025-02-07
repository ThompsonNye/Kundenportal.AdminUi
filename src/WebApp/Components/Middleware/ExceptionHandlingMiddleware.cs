namespace Kundenportal.AdminUi.WebApp.Components.Middleware;

public class ExceptionHandlingMiddleware(
	RequestDelegate next,
	ILogger<ExceptionHandlingMiddleware> logger)
{
	private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
	private readonly RequestDelegate _next = next;

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(ex, context);
		}
	}

	private async Task HandleExceptionAsync(Exception exception, HttpContext context)
	{
		_logger.LogError(exception, "Unhandled error occurred in the application");

		if (context.Response.HasStarted) return;

		var result = TypedResults.Problem(statusCode: 500);

		context.Response.StatusCode = result.StatusCode;
		await context.Response.WriteAsJsonAsync(result.ProblemDetails);
	}
}
