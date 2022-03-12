using AppLogic.Accounts;
using AppLogic.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppLogic;

public static class BookingLogicStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IBookingValidationService, BookingValidationService>();

        services.AddMediatR(typeof(SearchAccountsQuery).Assembly);

        var assembliesToRegister = new List<Assembly>() { typeof(SearchAccountsQuery).Assembly };
        AssemblyScanner.FindValidatorsInAssemblies(assembliesToRegister).ForEach(pair =>
        {
            services.Add(ServiceDescriptor.Transient(pair.InterfaceType, pair.ValidatorType));
        });
    }
}
