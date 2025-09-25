using K4os.Compression.LZ4.Streams;
using Regulator.Services.Files.Shared.Services.Interfaces;

namespace Regulator.Services.Files.Shared.Services;

public class CompressionService : ICompressionService
{
    public async Task<string> CompressFileAsync(string filePath)
    {
        return await Task.Run(() => CompressFile(filePath));
    }

    public async Task<string> CompressFileFromStreamAsync(Stream stream)
    {
        return await Task.Run(() => CompressFileFromStream(stream));
    }

    public async Task<MemoryStream> CompressToStreamAsync(Stream stream)
    {
        return await Task.Run(() => CompressToStream(stream));
    }

    public async Task<MemoryStream> CompressToStreamAsync(string filePath)
    {
        return await Task.Run(() => CompressToStream(filePath));
    }

    public async Task<string> DecompressFileAsync(string filePath)
    {
        return await Task.Run(() => DecompressFile(filePath));
    }

    public async Task<string> DecompressFileFromStreamAsync(Stream stream, string destinationFilePath)
    {
        return await Task.Run(() => DecompressFileFromStream(stream, destinationFilePath));
    }

    public async Task<MemoryStream> DecompressToStreamAsync(Stream stream)
    {
        return await Task.Run(() => DecompressToStream(stream));
    }

    public async Task<MemoryStream> DecompressToStreamAsync(string filePath)
    {
        return await Task.Run(() => DecompressToStream(filePath));
    }
    
        private static string CompressFile(string filePath)
    {
        var targetFilePath = Path.GetTempFileName();
        
        using var source = File.OpenRead(filePath);
        using var target = LZ4Stream.Encode(File.Create(targetFilePath));
        source.CopyTo(target);
        return targetFilePath;
    }

    private static string CompressFileFromStream(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var targetFilePath = Path.GetTempFileName();
        
        using var target = LZ4Stream.Encode(File.Create(targetFilePath));
        stream.CopyTo(target);
        return targetFilePath;
    }

    private static MemoryStream CompressToStream(Stream stream)
    {
        var targetStream = new MemoryStream();
        
        using var lz4Stream = LZ4Stream.Encode(targetStream, leaveOpen: true);
        stream.CopyTo(lz4Stream);
        targetStream.Position = 0;
        
        return targetStream;
    }

    private static MemoryStream CompressToStream(string filePath)
    {
        var targetStream = new MemoryStream();
        
        using var source = File.OpenRead(filePath);
        using var lz4Stream = LZ4Stream.Encode(targetStream, leaveOpen: true);
        source.CopyTo(lz4Stream);
        targetStream.Position = 0;
        
        return targetStream;
    }

    private static string DecompressFile(string filePath)
    {
        var targetFilePath = Path.GetTempFileName();
        
        using var source = LZ4Stream.Decode(File.OpenRead(filePath));
        using var target = File.Create(targetFilePath);
        source.CopyTo(target);
        
        return targetFilePath;
    }

    private static string DecompressFileFromStream(Stream stream, string destinationFilePath)
    {
        using var source = LZ4Stream.Decode(stream);
        using var target = File.Create(destinationFilePath);
        source.CopyTo(target);
        
        return destinationFilePath;
    }

    private static MemoryStream DecompressToStream(Stream stream)
    {
        var targetStream = new MemoryStream();
        
        using var lz4Stream = LZ4Stream.Decode(stream);
        lz4Stream.CopyTo(targetStream);
        targetStream.Position = 0;
        
        return targetStream;
    }

    private static MemoryStream DecompressToStream(string filePath)
    {
        var targetStream = new MemoryStream();
        
        using var source = LZ4Stream.Decode(File.OpenRead(filePath));
        source.CopyTo(targetStream);
        targetStream.Position = 0;
        
        return targetStream;
    }
}