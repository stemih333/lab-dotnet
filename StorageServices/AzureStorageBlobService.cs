using Azure.Storage.Blobs;
using Common.Exceptions;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace StorageServices;

public class AzureStorageBlobService : IFileService
{
    private readonly IConfiguration _configuration;

    public AzureStorageBlobService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Stream> GetBlobStreamAsync(string filePath, string containerName)
    {
        var container = await GetBlobContainer(containerName);
        var fileName = Path.GetFileName(filePath);
        var client = container.GetBlobClient(fileName);
        if (!(await client.ExistsAsync()))
            throw new StorageException("File does not exist in file storage.");

        var stream = await client.DownloadStreamingAsync();
        return stream.Value.Content;
    }

    public async Task<byte[]> GetBlobBytesAsync(string filePath, string containerName)
    {
        var container = await GetBlobContainer(containerName);
        var fileName = Path.GetFileName(filePath);
        var client = container.GetBlobClient(fileName);

        if (!(await client.ExistsAsync()))
            throw new StorageException("File does not exist in file storage.");

        var stream = await client.DownloadContentAsync();
        return stream.Value.Content.ToArray();
    }

    public async Task<bool> DeleteFileAsync(string filePath, string containerName)
    {
        var container = await GetBlobContainer(containerName);
        var fileName = Path.GetFileName(filePath);
        var client = container.GetBlobClient(fileName);
        
        var res = await client.DeleteIfExistsAsync();
        return res.Value;
    }


    public async Task<string> SaveFileAsync(string filename, Stream stream, string containerName)
    {
        var container = await GetBlobContainer(containerName);
        var client = container.GetBlobClient(filename);
        if (await client.ExistsAsync())
            throw new StorageException("File already exists in storage.");
        await client.UploadAsync(stream);

        return client.Uri.ToString();
    }

    public async Task<string> SaveFileAsync(string filename, byte[] binaryData, string containerName)
    {
        var container = await GetBlobContainer(containerName);
        var client = container.GetBlobClient(filename);
        await client.UploadAsync(new BinaryData(binaryData));

        return client.Uri.ToString();
    }

    private async Task<BlobContainerClient> GetBlobContainer(string containerName = "bookings")
    {
        var service = new BlobServiceClient(_configuration["AzureWebJobsStorage"]);
        var container = service.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();
        return container;
    }
}
