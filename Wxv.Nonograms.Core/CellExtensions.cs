using Wxv.Core;

namespace Wxv.Nonograms.Core;

public static class CellExtensions
{
    public const string FlagCharacters = " X";
    public const string DisplayCharacters = " @.";
    public const string IsFinishedCharacter = "Y";

    public static string ToDisplayString(this Cell cell) => DisplayCharacters[(int) cell].ToString();
    public static string ToDisplayString(this IEnumerable<Cell> cells) => cells.Select(ToDisplayString).JoinString();
    
    public static Cell ParseCell(this char c) 
        => DisplayCharacters.Contains(c)
        ? (Cell)DisplayCharacters.IndexOf(c)
        : throw new InvalidDataException();
    public static Cell[] ParseCells(this string value) => value.Select(ParseCell).ToArray();

    public static bool ParseFlag(this char c) 
        => FlagCharacters.Contains(c)
        ? FlagCharacters.IndexOf(c) > 0
        : throw new InvalidDataException();

    public static bool[][] Clone(
        this IReadOnlyCollection<IReadOnlyCollection<bool>> value)
        => value
            .Select(completedSet => completedSet.ToArray())
            .ToArray();

    public static bool[][] Clear(
        this IReadOnlyCollection<IReadOnlyCollection<bool>> value)
        => value
            .Select(completedSet => completedSet.Select(_ => false).ToArray())
            .ToArray();

    public static Grid<Cell> Clone(this IReadonlyGrid<Cell> value)
        => new(
            value.Width,
            value.Height,
            (x, y) => value[x, y]);

    public static Grid<bool> Clone(this IReadonlyGrid<bool> value)
        => new(
            value.Width,
            value.Height,
            (x, y) => value[x, y]);
    public static Grid<bool> Clear(this IReadonlyGrid<bool> value)
        => new Grid<bool>(
            value.Width,
            value.Height,
            (_, _) => false);

    public static Grid<bool> CloneIfNotChanged(
        this IReadonlyGrid<bool> value, 
        IReadonlyGrid<Cell> oldCells, 
        IReadonlyGrid<Cell> newCells)
        => new Grid<bool>(
            value.Width,
            value.Height,
            (x, y) => oldCells[x, y] == newCells[x, y] && value[x, y]);

    public static bool AreCellsComplete(this IEnumerable<Cell> solution, IEnumerable<Cell> cells)
    {
        if (solution.Count() != cells.Count())
            throw new ArgumentOutOfRangeException();

        return Enumerable
            .Range(0, solution.Count())
            .All(i => (solution.ElementAt(i) == Cell.Set) == (cells.ElementAt(i) == Cell.Set));
    }

    public static int CountChanges(this Cell[] cells, Cell[] changes)
    {
        if (cells.Length != changes.Length)
            throw new InvalidOperationException();

        var count = 0;
        for (var i = 0; i < cells.Length; i++)
        {
            if (changes[i] == Cell.Unknown)
            {
                // no change
            }
            else if (cells[i] == Cell.Unknown)
            {
                // current cell is uknown, so this is a valid change 
                count++;
            }
            else if (cells[i] != changes[i])
            {
                // this is an invalid change
                return 0; 
            }
        }
        return count;
    }

    // is this these changes valid? 
    public static bool AreChangesValid(this Cell[] cells, Cell[] changes)
    {
        if (cells.Length != changes.Length)
            throw new InvalidOperationException();
        for (var i = 0; i < cells.Length; i++)
        {
            if (cells[i] == Cell.Unknown)
            {
                // current cell is unknown, so this is a valid change 
            }
            else if (cells[i] != changes[i])
            {
                // this is an invalid change
                return false; 
            }
        }
        return true;
    }

    /// <summary>
    /// Get the unset combinations of a set of unset spans of specific size (count) which sum equals the specified amount
    /// Leading and trailing unset sums can be zero, but seperations between set spans must be greater
    /// </summary>
    public static IEnumerable<int[]> GetUnsetLengthCombinations(this int unsetSum, int unsetLengthsCount)
    {
        if (unsetSum < 0)
            throw new ArgumentOutOfRangeException(nameof(unsetSum));
        if (unsetLengthsCount <= 1)
            throw new ArgumentOutOfRangeException(nameof(unsetLengthsCount));
        if (unsetSum == 0)
        {
            yield return Enumerable.Range(0, unsetLengthsCount).Select(_ => 0).ToArray();
            yield break;
        }
    
        var workingSet = Enumerable.Range(0, unsetLengthsCount).Select(_ => 0).ToArray();
        var combinations = GetUnsetLengthCombinations(unsetSum, unsetLengthsCount, 0, workingSet);
        foreach (var result in combinations)
            yield return result;
    }

    private static IEnumerable<int[]> GetUnsetLengthCombinations(this int unsetSum, int unsetLengthsCount, int unsetCombinationIndex, int[] workingSet)
    {
        var remainingSum = unsetSum - workingSet.Take(unsetCombinationIndex).Sum();
        
        // last combination value, use the remaining sum
        if (unsetCombinationIndex >= unsetLengthsCount - 1)
        {
            workingSet[unsetCombinationIndex] = remainingSum; 
            yield return workingSet.ToArray();
            yield break;
        }

        // iterate through the possible values
        var minCombinationValue = unsetCombinationIndex <= 0 ? 0 : 1;
        var maxCombinationValue = unsetCombinationIndex > 0 || unsetLengthsCount <= 2 
            ? remainingSum 
            : unsetSum - unsetLengthsCount + 2; 
        for (var combinationIndexValue = minCombinationValue; combinationIndexValue <= maxCombinationValue; combinationIndexValue++)
        {
            workingSet[unsetCombinationIndex] = combinationIndexValue;
            var combinations = unsetSum.GetUnsetLengthCombinations(
                unsetLengthsCount,
                unsetCombinationIndex + 1,
                workingSet);
            foreach (var combination in combinations)
                yield return combination;
        }
    }

    public static IEnumerable<Cell[]> GetValidCellCombinations(this int cellsCount, int[] lengths)
    {
        if (cellsCount < 1)
            throw new ArgumentOutOfRangeException();

        if (!lengths.Any())
        {
            yield return Enumerable.Range(0, cellsCount).Select(_ => Cell.Unset).ToArray();
            yield break;
        }
        
        var lengthsSum = lengths.Sum();
        if (lengthsSum > cellsCount)
            throw new ArgumentOutOfRangeException();
        
        if (lengthsSum == cellsCount)
        {
            yield return Enumerable.Range(0, cellsCount).Select(_ => Cell.Set).ToArray();
            yield break;
        }

        var unsetSum = cellsCount - lengths.Sum();
        var unsetLengthsCount = lengths.Length + 1;
        var unsetCombinations = GetUnsetLengthCombinations(unsetSum, unsetLengthsCount);
        foreach (var unsetCombination in unsetCombinations)
        {
            var result = new List<Cell>();
            for (var i = 0; i < lengths.Length; i++)
            {
                result.AddRange(Enumerable.Range(0, unsetCombination[i]).Select(_ => Cell.Unset));
                result.AddRange(Enumerable.Range(0, lengths[i]).Select(_ => Cell.Set));
            }
            result.AddRange(Enumerable.Range(0, unsetCombination.Last()).Select(_ => Cell.Unset));

            if (result.Count != cellsCount)
                throw new InvalidOperationException();

            yield return result.ToArray();
        }
    }
    
    /// <summary>
    /// Is it complete and correct? 
    /// </summary>
    public static bool IsSolved(this Cell[] cells, int[] lengths)
    {
        if (cells.Any(c => c == Cell.Unknown))
            return false;
        
        if (!lengths.Any())
            return cells.All(c => c == Cell.Unset);
        
        return cells
            .GetConsecutive()
            .Where(c => c.Value == Cell.Set) 
            .Select(c => c.Count)
            .SequenceEqual(lengths);
    }

    /// <summary>
    /// Try an validate the cells.
    /// Returns false if definetely not valid.
    /// If it returns true, it may still not be correct, but it simply can't validate it fully
    /// </summary>
    public static bool Validate(this Cell[] cells, int[] lengths, CancellationToken cancellationToken)
    {
        if (lengths.Sum() + lengths.Length - 1 > cells.Length)
            throw new ArgumentOutOfRangeException(nameof(cells));

        if (!lengths.Any())
            return cells.All(c => c != Cell.Set);

        if (cells.Count(c => c == Cell.Set) > lengths.Sum())
            return false;
        
        // is set count bigger then expected?
        if (cells.Count(c => c == Cell.Set) > lengths.Sum())
            return false;
        
        // is unset count bigger then expected?
        if (cells.Count(c => c == Cell.Unset) > cells.Length - lengths.Sum())
            return false;

        var consecutiveCells = cells
            .GetConsecutive()
            .ToArray();
        var consecutiveSetCells = consecutiveCells
            .Where(c => c.Value == Cell.Set) 
            .ToArray();
        
        // are the set cells spans correct? (is complete)
        if (consecutiveSetCells.Select(c => c.Count).SequenceEqual(lengths))
            return true;

        // if there are no unknowns then it should be correct
        if (cells.All(c => c != Cell.Unknown))
            return false;
            
        // check to see if any of the set spans don't exceed the max length
        var maxSetLength = lengths.Max();
        if (consecutiveSetCells.Any(cc => cc.Count > maxSetLength))
            return false;

        var consecutiveUnsetCells = consecutiveCells
            .Where(c => c.Value == Cell.Unset) 
            .ToArray();

        // check to see if any of the unset spans don't exceed the max length
        var unsetCount = cells.Length - lengths.Sum();
        if (consecutiveUnsetCells.Any(cc => cc.Count > unsetCount))
            return false;
        
        // get the spans of sets or unkowns (divided by unset)
        // if the counts of the spans containing sets is larger, then it's invalid
        // if the counts of the spans containing sets and the lengths are equal, make everything fits 
        {
            var concequativeSetOrUknowns = cells
                .Select(c => c != Cell.Unset)
                .GetConsecutive()
                .ToArray();
            var setsWithUnknowns = concequativeSetOrUknowns
                .Where(c => c.Value && cells.Skip(c.Index).Take(c.Count).Any(c0 => c0 == Cell.Set))
                .Select(c => cells.Skip(c.Index).Take(c.Count).ToArray())
                .ToArray();
            if (setsWithUnknowns.Length > lengths.Length)
                return false; // it's invalid
            if (setsWithUnknowns.Length == lengths.Length)
            {
                if (Enumerable.Range(0, setsWithUnknowns.Length).Any(i => setsWithUnknowns[i].Length < lengths[i]))
                    return false; // this span can't hold the length
                // trim of unknowns
                var setsWithUnknownsTrimmed = setsWithUnknowns
                    .Select(span => span.SkipWhile(c => c == Cell.Unknown).Reverse().SkipWhile(c => c == Cell.Unknown).Reverse().ToArray())
                    .ToArray();
                if (Enumerable.Range(0, setsWithUnknowns.Length).Any(i => setsWithUnknownsTrimmed[i].Length > lengths[i]))
                    return false; // this span can't hold the length
            }
        }

        if (!cells.TryRemoveSolved(lengths,
          out var unsolvedCells,
          out var unsolvedLengths,
          out _,
          out _))
        {
            unsolvedCells = cells;
            unsolvedLengths = lengths;
        }

        var validCombinations = unsolvedCells!.Length.GetValidCellCombinations(unsolvedLengths!);
        foreach (var validCombination in validCombinations)
        {
            if (unsolvedCells.AreChangesValid(validCombination))
                return true;

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();
        }

        return false;
    }

    public static bool TryRemoveSolved(
        this Cell[] cells, 
        int[] lengths, 
        out Cell[]? unsolvedCells, 
        out int[]? unsolvedLengths,
        out Cell[]? removedPrefix, 
        out Cell[]? removedSuffix)
    {
        unsolvedCells = null;
        unsolvedLengths = null;
        removedPrefix = null;
        removedSuffix = null;
        if (cells.IsSolved(lengths))
            return false;

        var workingCells = cells.ToArray();

        var changed = false;
        if (TryRemoveSolvedPrefix(workingCells, lengths, out unsolvedCells, out unsolvedLengths, out var changeRemoved))
        {
            workingCells = unsolvedCells!.ToArray();
            lengths = unsolvedLengths!.ToArray();
            removedPrefix = changeRemoved!;
            changed = true;
        }

        if (TryRemoveSolvedPrefix(workingCells.Reverse().ToArray(), lengths.Reverse().ToArray(), out unsolvedCells, out unsolvedLengths, out changeRemoved))
        {
            workingCells = unsolvedCells!.Reverse().ToArray();
            lengths = unsolvedLengths!.Reverse().ToArray();
            removedSuffix = changeRemoved!.Reverse().ToArray();
            changed = true;
        }

        if (changed)
        {
            removedPrefix ??= Array.Empty<Cell>();
            removedSuffix ??= Array.Empty<Cell>();

            if (removedPrefix.Length + workingCells.Length + removedSuffix.Length != cells.Length)
                throw new InvalidOperationException();

            unsolvedCells = workingCells;
            unsolvedLengths = lengths;
        }

        return changed;
    }
    
    private static bool TryRemoveSolvedPrefix(
        this Cell[] cells, 
        int[] lengths, 
        out Cell[]? unsolvedCells, 
        out int[]? unsolvedLengths, 
        out Cell[]? removedCells)
    {
        unsolvedCells = null;
        unsolvedLengths = null;
        removedCells = null;

        var unsolvedCellsList = cells.ToList();
        var unsolvedLengthsList = lengths.ToList();
        var removedCellsList = new List<Cell>();
        var consecutives = cells.GetConsecutive().ToArray();

        var changed = false;
        var lengthIndex = 0;
        var consecutiveIndex = 0;
        while (consecutiveIndex < consecutives.Length)
        {
            var consecutive = consecutives[consecutiveIndex];
            // unknown span, stop
            if (consecutive.Value == Cell.Unknown)
                break;

            // remove unset
            if (consecutive.Value == Cell.Unset)
            {
                removedCellsList.AddRange(unsolvedCellsList.Take(consecutive.Count));
                unsolvedCellsList = unsolvedCellsList.Skip(consecutive.Count).ToList();
                consecutiveIndex++;
                changed = true;
                continue;
            }
            
            // this is a set span
            
            // we found more set spans then expected, the cells must be invalid
            if (lengthIndex >= lengths.Length)
                return false;
            
            // set span with a lesser length, so stop
            if (consecutive.Count < lengths[lengthIndex])
                break;
            
            // the next span is unknown, so the length is uncertain
            if (!consecutive.IsLast && consecutive.Next!.Value == Cell.Unknown)
                break;

            // this is an invalid or incomplete length, cancel the operation
            if (consecutive.Count != lengths[lengthIndex])
                return false;
            
            // remove set
            removedCellsList.AddRange(unsolvedCellsList.Take(consecutive.Count));
            unsolvedCellsList = unsolvedCellsList.Skip(consecutive.Count).ToList();
            unsolvedLengthsList.RemoveAt(0);
            lengthIndex++;
            consecutiveIndex++;
            
            changed = true;
        }

        if (!changed)
            return false;

        unsolvedCells = unsolvedCellsList.ToArray();
        unsolvedLengths = unsolvedLengthsList.ToArray();
        removedCells = removedCellsList.ToArray();
        return true;
    }
    

}