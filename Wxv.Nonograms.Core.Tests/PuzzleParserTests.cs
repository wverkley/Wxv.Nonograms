using Xunit;
using Xunit.Abstractions;

namespace Wxv.Nonograms.Core.Tests;

public class PuzzleParserTests
{
    public ITestOutputHelper Logger { get; }

    public PuzzleParserTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }

    [Fact]
    public void Parse()
    {
        var puzzleParser = new PuzzleParser();
        var puzzle = puzzleParser.Parse(@"
#Puzzle
# Test 1
# Test 2
H:
[5]
[1]
[1]
[1]
[5]
V:
[1 1]
[1 2]
[1 1 1]
[2 1]
[1 1]
");
        
        Logger.WriteLine(puzzle.ToString());
    }

    [Fact]
    public void FromSolution()
    {
        var solution = Solution.Parse(@"
#Solution
[     @@@@@@   ]
[    @@@@@@@@  ]
[    @  @@@@@@ ]
[     @@@ @@@@ ]
[       @ @    ]
[    @@@@  @@  ]
[  @@@ @@@  @@ ]
[ @@ @@@@@@@@@@]
[ @@@@@@ @@ @@@]
[@@ @@   @@@@ @]
[ @@    @@@@@@@]
[  @@ @ @@@ @@@]
[  @    @@@@@@ ]
[ @@  @@ @@  @ ]
[  @@@   @     ]");
        var puzzle0 = solution.ToPuzzle();

        Logger.WriteLine(solution.ToString());
        Logger.WriteLine(puzzle0.ToString());

        var puzzleParser = new PuzzleParser();
        var puzzle1 = puzzleParser.Parse(puzzle0.ToString());

        Assert.Equal(puzzle0.ToString(), puzzle1.ToString());
    }
}