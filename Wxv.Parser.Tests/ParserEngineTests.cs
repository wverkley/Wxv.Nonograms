using Wxv.Parser.IO;
using Xunit.Abstractions;

namespace Wxv.Parser.Tests;

public class ParserEngineTests
{
    private const string CommonRules = @"
-seperator := ( ""[ \t]+"" )+.
integer := ""\-""? ""\d+"".
string := ""[\S]+"".
x := integer.
y := integer.
";
    // (horizontal size, vertical size)
    private static readonly Lazy<ParserEngine> DimensionParser = 
        new Lazy<ParserEngine>(() => new ParserReader().Read(CommonRules + @"
width := integer.
height := integer.
@root := ""\("" seperator? width seperator? ""\,"" seperator? height seperator? ""\)""? @eof."));

    public ITestOutputHelper Logger { get; }

    public ParserEngineTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }
    
    [Fact]
    public void Simple()
    {
        var result = DimensionParser.Value.Parse("(12, 34)");
        Assert.True(result.IsMatch);
        
        Logger.WriteLine(result.ToString());
    }
}