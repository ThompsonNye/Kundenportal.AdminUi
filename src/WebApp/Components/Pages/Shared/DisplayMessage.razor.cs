using Microsoft.AspNetCore.Components;

namespace Kundenportal.AdminUi.WebApp.Components.Pages.Shared;

public partial class DisplayMessage
{
    private static readonly Dictionary<DisplayMessageType, string> AlertTypesMapping = new()
    {
        [DisplayMessageType.Primary] = "primary",
        [DisplayMessageType.Secondary] = "secondary",
        [DisplayMessageType.Info] = "info",
        [DisplayMessageType.Success] = "success",
        [DisplayMessageType.Warning] = "warning",
        [DisplayMessageType.Danger] = "danger"
    };
    
    private static readonly Dictionary<DisplayMessageType, string> AlertIconsMapping = new()
    {
        [DisplayMessageType.Primary] = "info-circle",
        [DisplayMessageType.Secondary] = "info-circle",
        [DisplayMessageType.Info] = "info-circle",
        [DisplayMessageType.Success] = "check-circle",
        [DisplayMessageType.Warning] = "exclamation-circle",
        [DisplayMessageType.Danger] = "x-circle"
    };
    
    [Parameter]
    public string? Message { get; set; }
    
    [Parameter]
    [EditorRequired]
    public DisplayMessageType Type { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Whether to show a X which closes the alert.
    /// </summary>
    [Parameter] public bool ShowCloseX { get; set; } = true;
    
    private string AlertType => AlertTypesMapping.TryGetValue(Type, out string? alertType) ? alertType : "";
    
    private string AlertIcon => AlertIconsMapping.TryGetValue(Type, out string? alertIcon) ? alertIcon : "";

    private Task OnCloseClickedAsync()
    {
        return !OnClose.HasDelegate ? Task.CompletedTask : OnClose.InvokeAsync();
    }
}

public enum DisplayMessageType
{
    Primary,
    Secondary,
    Info,
    Success,
    Warning,
    Danger
}
