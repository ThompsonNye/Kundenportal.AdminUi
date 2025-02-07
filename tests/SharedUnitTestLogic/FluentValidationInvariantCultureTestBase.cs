using System.Globalization;
using FluentValidation;

namespace SharedUnitTestLogic;

public abstract class FluentValidationInvariantCultureTestBase
{
	protected FluentValidationInvariantCultureTestBase()
	{
		ValidatorOptions.Global.LanguageManager.Culture
			= CultureInfo.InvariantCulture;
	}
}
