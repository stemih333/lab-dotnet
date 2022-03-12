namespace Common.Interfaces;

public interface IFileService
{
    Task<bool> DeleteFileAsync(string filePath, string container = "default");
    Task<string> SaveFileAsync(string filename, Stream stream, string container = "default");
    Task<string> SaveFileAsync(string filename, byte[] binaryData, string container = "default");
    Task<Stream> GetBlobStreamAsync(string filePath, string containerName);
    Task<byte[]> GetBlobBytesAsync(string filePath, string containerName);

}
