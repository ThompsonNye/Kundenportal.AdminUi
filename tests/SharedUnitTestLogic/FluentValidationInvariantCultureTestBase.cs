using System.Globalization;

namespace SharedUnitTestLogic;

public abstract class FluentValidationInvariantCultureTestBase
{
    protected FluentValidationInvariantCultureTestBase()
    {
        FluentValidation.ValidatorOptions.Global.LanguageManager.Culture
            = CultureInfo.InvariantCulture;
    }
}