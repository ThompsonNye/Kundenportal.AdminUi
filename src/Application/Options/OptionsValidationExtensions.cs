using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kundenportal.AdminUi.Application.Options;

public static class OptionsValidationExtensions
{
	/// <summary>
	///     Adds fluent validation to the options.
	/// </summary>
	/// <typeparam name="TOptions"></typeparam>
	/// <param name="optionsBuilder"></param>
	/// <returns></returns>
	public static OptionsBuilder<TOptions> ValidateFluently<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
		where TOptions : class
	{
		optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(services =>
		{
			var validators = services.GetRequiredService<IEnumerable<IValidator<TOptions>>>();
			return new FluentOptionsValidation<TOptions>(optionsBuilder.Name, validators);
		});
		return optionsBuilder;
	}
}
