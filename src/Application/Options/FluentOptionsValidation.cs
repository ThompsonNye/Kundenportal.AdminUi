using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace Kundenportal.AdminUi.Application.Options;

public class FluentOptionsValidation<TOptions>(
	string name,
	IEnumerable<IValidator<TOptions>> validators)
	: IValidateOptions<TOptions>
	where TOptions : class
{
	/// <summary>
	///     The options name.
	/// </summary>
	public string Name { get; } = name;

	public ValidateOptionsResult Validate(string? name, TOptions options)
	{
		if (Name != name)
			// Ignored if not validating this instance.
			return ValidateOptionsResult.Skip;

		// Ensure options are provided to validate against
		ArgumentNullException.ThrowIfNull(options);

		ValidationResult[] validationResults = validators
			.Select(x => x.Validate(options))
			.ToArray();

		if (validationResults.Length == 0) return ValidateOptionsResult.Skip;

		if (validationResults.All(x => x.IsValid)) return ValidateOptionsResult.Success;

		string typeName = options.GetType().Name;
		List<string> errors = new List<string>();
		foreach (ValidationFailure? error in validationResults.SelectMany(x => x.Errors))
			errors.Add($"Fluent validation failed for options '{typeName}': {error.ErrorMessage}.");

		return ValidateOptionsResult.Fail(errors);
	}
}
