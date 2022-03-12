using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Auth;

public static class AuthStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IAuthService, AuthService>();
    }

    public static void ConfigureDevelopmentServices(IServiceCollection services)
    {
        services.AddTransient<IAuthService, DevAuthService>();
    }
}
