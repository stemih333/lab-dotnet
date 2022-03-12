namespace BlazorComponents.GuiModels;

public class AlertMessage
{
    public string Message { get; set; }
    public int Duration { get; set; } = 5000;
    public string CssClass { get; set; } = "alert-primary";
}
