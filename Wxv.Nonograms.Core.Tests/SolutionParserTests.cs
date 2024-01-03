using Xunit;
using Xunit.Abstractions;

namespace Wxv.Nonograms.Core.Tests;

public class SolutionParserTests
{
    public ITestOutputHelper Logger { get; }

    public SolutionParserTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }

    [Fact]
    public void Parse()
    {
        var solutionParser = new SolutionParser();
        var solution = solutionParser.Parse(@"
#Solution

# Test 1
# Test 2

[@@@@@]
[   @ ]
[  @  ]
[ @   ]
[@@@@@]");
        
        Logger.WriteLine(solution.ToString());
    }
}