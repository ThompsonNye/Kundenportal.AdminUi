using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Shared;

/// <summary>
///     Validates a string such that it does not match the given regular expression.
/// </summary>
public class NegativeRegularExpressionAttribute : ValidationAttribute
{
	private readonly string _regularExpression;

	public NegativeRegularExpressionAttribute([StringSyntax("Regex")] string regularExpression)
	{
		_regularExpression = regularExpression;
	}

	public override bool IsValid(object? value)
	{
		if (value is not string text)
		{
			return true;
		}

		return !Regex.IsMatch(text, _regularExpression);
	}
}
