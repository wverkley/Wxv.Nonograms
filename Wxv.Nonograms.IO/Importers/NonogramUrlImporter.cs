using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wxv.Core;

namespace Wxv.Nonograms.IO.Importers;

// view-source:https://www.nonograms.org/nonograms/i/8543
// <a class="lightbox" href="https://static.nonograms.org/files/nonograms/large/olen16_12_1_1p.png" target="_blank" id="nonogram_answer" title="View answer">Answer</a>

/// <summary>
/// Imports a puzzle from nonograms.org
/// </summary>
public class NonogramUrlImporter : INonogrameImporter
{
    public string Name => "Nonogram URL";

    private const string UrlPattern = @"^https:\/\/www\.nonograms\.org\/nonograms\/i\/\d+$";
    private static Regex UrlRegex { get; } = new(UrlPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
    
    private Func<string, CancellationToken, Task<string>> GetHtmlPage { get; }
    private Func<string, CancellationToken, Task<byte[]>> GetAnswerImage { get; }

    public NonogramUrlImporter(
        Func<string, CancellationToken, Task<string>> getHtmlPage, 
        Func<string, CancellationToken, Task<byte[]>> getAnswerImage)
    {
        GetHtmlPage = getHtmlPage;
        GetAnswerImage = getAnswerImage;
    }

    public NonogramUrlImporter() : this(
        (url, cancellationToken) => new HttpClient().GetStringAsync(url, cancellationToken),
        (url,cancellationToken) => new HttpClient().GetByteArrayAsync(url, cancellationToken))
    {
    }

    public async Task<NonogrameImporterResult> TryImportAsync(object data, CancellationToken cancellationToken, string[]? description)
    {
        var result = new NonogrameImporterResult
        {
            IsSuccess = false,
            Solution = null
        };
        try
        {
            var url = data as string;
            if (string.IsNullOrWhiteSpace(url) || !UrlRegex.IsMatch(url))
                return result;

            var html = await GetHtmlPage(url, cancellationToken);

            var htmlRange = html.Range();
            var answerImageUrl = htmlRange
                .AllBetween("<a ", "</a>")
                .FirstOrDefault(sr => !sr.StartsWith("id=\"nonogram_answer\"").IsNull())
                ?.Within("href=\"", "\"")
                .ToString();
            if (answerImageUrl == null)
                return result;

            var bitmapBytes = await GetAnswerImage(answerImageUrl, cancellationToken);
            
            return await new AnswerImageImporter().TryImportAsync(bitmapBytes, cancellationToken, description);
        }
        catch
        {
            return result;
        }
    }
}