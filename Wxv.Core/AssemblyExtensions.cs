using System.Reflection;

namespace Wxv.Core;

public static class AssemblyExtensions
{
    public static string GetManifestResourceString(this Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
}