using Azure.Storage.Queues;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace StorageServices;

public class AzureStrorageQueueService : IQueueService
{
    private readonly IConfiguration _configuration;

    public AzureStrorageQueueService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task InsertMessageToQueue(string message, string queueName)
    {
        var client = await GetQueueClient(queueName);
        await client.SendMessageAsync(message);
    }

    private async Task<QueueClient> GetQueueClient(string queueName)
    {
        var client = new QueueClient(_configuration["AzureWebJobsStorage"], queueName, new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        });
        await client.CreateIfNotExistsAsync();        

        return client;
    }
}
