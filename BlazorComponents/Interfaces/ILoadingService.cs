namespace BlazorComponents.Interfaces;

public interface ILoadingService
{
    event Func<int, Task> OnHttpChanged;
    Task StartedHttpRequest();
    Task FinishedHttpRequest();
}
