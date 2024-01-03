using System.Collections.Generic;

namespace Wxv.Nonograms.IO.Importers;

public static class Registry
{
    public static IReadOnlyCollection<INonogrameImporter> Importers { get; } = new INonogrameImporter[]
    {
        new SolutionImporter(),
        new AnswerImageImporter(),
        new SolutionImageImporter(),
        new NonogramUrlImporter(),
    };
    
}