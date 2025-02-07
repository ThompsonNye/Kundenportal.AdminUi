using System.Globalization;

namespace Kundenportal.AdminUi.WebApp.Endpoints;

/// <summary>
/// Sets the current culture to en-US and resets it to the previously set culture in the <see cref="Dispose"/> method.
/// </summary>
public sealed class EnUsCulture : IDisposable
{
    private readonly CultureInfo _currentCulture;
    private readonly CultureInfo _currentUiCulture;

    public EnUsCulture()
    {
        _currentCulture = CultureInfo.CurrentCulture;
        _currentUiCulture = CultureInfo.CurrentUICulture;
        
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
    }
    
    public void Dispose()
    {
        CultureInfo.CurrentCulture = _currentCulture;
        CultureInfo.CurrentUICulture = _currentUiCulture;
    }
}
