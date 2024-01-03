using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wxv.Nonograms.Core;

namespace Wxv.Nonograms.IO.Importers;

public struct NonogrameImporterResult
{
    public bool IsSuccess { get; internal set; }
    public Solution? Solution { get; internal set; }
}

public interface INonogrameImporter
{
    string Name { get; }

    Task<NonogrameImporterResult> TryImportAsync(
      object data, 
      CancellationToken cancellationToken, 
      string[]? description);
}

public static class NonogrameImporterExtensions
{
    public static Task<NonogrameImporterResult> TryImportAsync(
      this INonogrameImporter importer, 
      object data, CancellationToken cancellationToken, 
      string? description = null)
        => importer.TryImportAsync(data, cancellationToken, description?.Split('\\').Select(s => s.TrimEnd()).ToArray());
} 