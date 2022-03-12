using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace StorageServices;

public class StorageServicesStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IQueueService, AzureStrorageQueueService>();
        services.AddScoped<IFileService, AzureStorageBlobService>();

    }
}
