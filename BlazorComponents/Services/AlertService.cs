namespace BlazorComponents.Services;

public class AlertService : IAlertService
{
    public event Func<AlertMessage, Task> OnAlertMessageChanged;

    public Task SetAlertMessage(AlertMessage message)
    {
        return OnAlertMessageChanged(message);
    }
}
