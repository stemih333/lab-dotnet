using AppLogic;
using Auth;
using Common;
using Database;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Models.Values;
using Serilog;
using StorageServices;
using System;
using System.Globalization;

[assembly: FunctionsStartup(typeof(Functions.Startup))]

namespace Functions;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddOptions<AppOptions>()
            .Configure<IConfiguration>((opts, configuration) =>
            {
                configuration
                .GetSection("AppOptions")
                .Bind(opts);
            });

        var logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .MinimumLevel.Warning()
                        .CreateLogger();
        builder.Services.AddLogging(lb => lb.AddSerilog(logger));

        var connectionString = Environment.GetEnvironmentVariable("AppOptions:BookingDbConnectionString");

        CommonAppLogicStartup.ConfigureServices(builder.Services);
        BookingLogicStartup.ConfigureServices(builder.Services);
        DbStartup.ConfigureServices(builder.Services, connectionString);
        StorageServicesStartup.ConfigureServices(builder.Services);
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        if (builder.GetContext().EnvironmentName == "Development")
        {
            AuthStartup.ConfigureDevelopmentServices(builder.Services);
        }
        else
        {
            AuthStartup.ConfigureServices(builder.Services);
        }
    }
}
