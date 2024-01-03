using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wxv.Nonograms.Core;

namespace Wxv.Nonograms.IO.Importers;

public class SolutionImporter : INonogrameImporter
{
    public string Name => "Solution";

    public Task<NonogrameImporterResult> TryImportAsync(object data, CancellationToken cancellationToken, string[]? description)
    {
        var result = Task.FromResult(new NonogrameImporterResult
        {
            IsSuccess = false,
            Solution = null
        });        
        
        try
        {
            var content = data as string;
            if (string.IsNullOrWhiteSpace(content))
                return result;

            try
            {
                // try raw string
                return Task.FromResult(new NonogrameImporterResult
                {
                    IsSuccess = true,
                    Solution = Solution.Parse(content)
                });
            }
            catch
            {
                // try base64 encoded string
                content = Encoding.UTF8.GetString(Convert.FromBase64String(content));

                return Task.FromResult(new NonogrameImporterResult
                {
                    IsSuccess = true,
                    Solution = Solution.Parse(content)
                });
            }
        }
        catch
        {
            return result;
        }
    }
}