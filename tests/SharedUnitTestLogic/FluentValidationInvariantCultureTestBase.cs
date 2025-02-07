using FluentValidation;
using System.Globalization;

namespace SharedUnitTestLogic;

public abstract class FluentValidationInvariantCultureTestBase
{
	protected FluentValidationInvariantCultureTestBase()
	{
		ValidatorOptions.Global.LanguageManager.Culture
			= CultureInfo.InvariantCulture;
	}
}
