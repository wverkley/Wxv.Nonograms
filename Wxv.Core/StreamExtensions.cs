using System.Text;

namespace Wxv.Core;

public static class StreamExtensions
{
    public static async Task<byte[]> ReadAllBytes(this Stream stream)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
    
    public static async Task<string> ReadAllText(this Stream stream, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = await stream.ReadAllBytes();
        return encoding.GetString(bytes);
    }
}