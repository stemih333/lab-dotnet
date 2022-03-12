using System.Reflection;

namespace BlazorComponents.Services;

public abstract class BaseHttpService
{
    private readonly ILoadingService _loadingService;

    protected BaseHttpService(ILoadingService loadingService)
    {
        _loadingService = loadingService;
    }

    protected async Task<M> RunRequest<M>(Func<Task<M>> func)
    {
        try
        {
            await _loadingService.StartedHttpRequest();
            return await func();

        }
        finally
        {
            await _loadingService.FinishedHttpRequest();
        }
    }

    protected static Dictionary<string, string> GetDictionaryFromSimpleClass(object model)
	{
        return model.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(model, null)?.ToString() ?? "");

    }
}
