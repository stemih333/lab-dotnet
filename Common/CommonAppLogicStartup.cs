using Common.Interfaces;
using Common.Mediatr;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
namespace Common;

public static class CommonAppLogicStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour<,>));
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }
}
