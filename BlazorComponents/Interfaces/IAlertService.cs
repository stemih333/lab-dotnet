namespace BlazorComponents.Interfaces;

public interface IAlertService
{
    event Func<AlertMessage, Task> OnAlertMessageChanged;
    Task SetAlertMessage(AlertMessage message);
}
