using Xunit;
using Xunit.Abstractions;

namespace Wxv.Nonograms.Core.Tests;

public class SolverEngineTests
{
    public ITestOutputHelper Logger { get; }

    public SolverEngineTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }

    [Theory]
    [InlineData(@"
#Solution
[@@@@@]
[   @ ]
[  @  ]
[ @   ]
[@@@@@]")]
    [InlineData(@"
#Solution
[ @@@ ]
[@   @]
[ @@@ ]
[  @  ]
[  @  ]
[ @@  ]
[@@@  ]")]
    [InlineData(@"
#Solution
[ @@@ ]
[@   @]
[@   @]
[    @]
[@@@@@]
[@@ @@]
[@@@@@]
[@@@@@]
[ @@@ ]")]
    [InlineData(@"
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
[  @@@   @     ]")]
    public void SolveFromSolution(string solutionString)
    {
        var solution = Solution.Parse(solutionString);
        Logger.WriteLine(Environment.NewLine + solution);

        var puzzle = solution.ToPuzzle();
        Logger.WriteLine(Environment.NewLine + puzzle);

        var turn = solution.ToTurn();
        
        Logger.WriteLine(Environment.NewLine + turn);

        try
        {
            var solverEngine = new SolverEngine(solution: solution);
            SolverEngineResult? solverEngineResult;
            while ((solverEngineResult = solverEngine.Solve(puzzle, turn, CancellationToken.None)) != null)
            {
                turn = solverEngineResult.NextTurn;
                Logger.WriteLine(Environment.NewLine + solverEngineResult);
            }

            Logger.WriteLine(Environment.NewLine + turn);

            Assert.True(turn.IsComplete());        
        }
        catch (SolverEngineInvalidException ex)
        {
            Logger.WriteLine("INVALID:");
            Logger.WriteLine(ex.ToString());
            throw;
        }
    }

    [Theory]
    [InlineData(@"
#Solution
[@ ]
[ @]")]
    [InlineData(@"
#Solution
[@@  ]
[@@  ]
[  @@]
[  @@]")]
    public void Unsolveable(string solutionString)
    {
        var solution = Solution.Parse(solutionString);
        Logger.WriteLine(Environment.NewLine + solution);

        var puzzle = solution.ToPuzzle();
        Logger.WriteLine(Environment.NewLine + puzzle);
        
        var turn = solution.ToTurn();
        
        Logger.WriteLine(Environment.NewLine + turn);

        try
        {
            var solverEngine = new SolverEngine(solution: solution);
            SolverEngineResult? solverEngineResult;
            while ((solverEngineResult = solverEngine.Solve(puzzle, turn, CancellationToken.None)) != null)
            {
                turn = solverEngineResult.NextTurn;
                Logger.WriteLine(Environment.NewLine + solverEngineResult);
            }

            Assert.False(turn.IsComplete());        
        }
        catch (SolverEngineInvalidException ex)
        {
            Logger.WriteLine("INVALID:");
            Logger.WriteLine(ex.ToString());
            throw;
        }
    }


}