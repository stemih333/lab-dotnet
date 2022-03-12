namespace Common.Interfaces;

public interface IQueueService
{
    Task InsertMessageToQueue(string message, string queueName = "default");
}
