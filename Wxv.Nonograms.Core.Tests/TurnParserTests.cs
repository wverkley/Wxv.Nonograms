using Xunit;
using Xunit.Abstractions;

namespace Wxv.Nonograms.Core.Tests;

public class TurnParserTests
{
    public ITestOutputHelper Logger { get; }

    public TurnParserTests(ITestOutputHelper logger)
    {
        Logger = logger;
    }

    [Fact]
    public void Parse()
    {
        var turnParser = new TurnParser();
        var turn = turnParser.Parse(@"
#Turn
H:
[X]
[X]
[X]
[X]
[ ]
V:
[X ]
[XX ]
[XX ]
[X ]
[X ]
C:
[@@@@@]
[   @ ]
[  @  ]
[ @   ]
[.....]
W:
[     ]
[     ]
[     ]
[     ]
[     ]
I:
[     ]
[     ]
[     ]
[     ]
[     ]
");
        
        Logger.WriteLine(turn.ToString());
    }
}