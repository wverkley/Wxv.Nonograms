using System.Collections;

namespace Wxv.Parser;

public class ParserEngine : IEnumerable<ParserSymbol>
{
    private Dictionary<string, ParserSymbol> Symbols { get; } = new();

    public ParserSymbol this[string name] => Symbols[name]; 
    public int Count => Symbols.Count(); 
    public ParserSymbol? RootSymbol => Symbols.Values.FirstOrDefault(s => s.IsRoot); 
    public bool ContainsSymbol(string name) => Symbols.ContainsKey(name); 

    internal ParserEngine() { }

    internal void AddSymbol(ParserSymbol symbol)
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));

        if (Symbols.ContainsKey(symbol.Name))
            throw new InvalidOperationException("Duplicate symbol name");

        if (symbol.IsRoot && RootSymbol != null)
            throw new InvalidOperationException("Duplicate root symbol");

        Symbols.Add(symbol.Name, symbol);
    }

    public override string ToString() 
        => string.Join(Environment.NewLine, Symbols.Values.Select(s => s.ToString()));

    public IEnumerator<ParserSymbol> GetEnumerator() 
        => Symbols.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() 
        => Symbols.Values.GetEnumerator();

    public ParserResult Parse(string content, bool removeAnonymous = true)
    {
        if (string.IsNullOrEmpty(content))
            throw new ArgumentNullException(nameof(content));

        var result = ParseSymbol(RootSymbol, content, ParserPosition.Start) 
             ?? new ParserResult
             {
                 Symbol = RootSymbol,
                 Content = content,
                 StartPosition = ParserPosition.Start,
                 EndPosition = ParserPosition.Start,
                 CanFallback = false,
                 IsMatch = false,
                 Children = null
             };
        if (removeAnonymous)
            result.RemoveAnonymous();
        return result;
    }

    private ParserResult? ParseSymbol(
        ParserSymbol symbol, 
        string content, 
        ParserPosition startPosition)
    {
        //Console.WriteLine("Checking " + symbol.Name);
        foreach(var group in symbol)
        {
            var endPosition = startPosition;

            bool groupIsMatch;
            if ((groupIsMatch = ParseGroup(
                group, 
                content, 
                ref endPosition, 
                out var groupResult, 
                out var groupCanFallback))
              || !groupCanFallback)
            {
                var result = new ParserResult
                {
                    Symbol = symbol,
                    Content = content,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    CanFallback = groupCanFallback,
                    IsMatch = groupIsMatch,
                    Children = groupResult
                };
                //Console.WriteLine("Found: " + result.Name);
                return result;
            }
        }
        return null;
    }

    private bool ParseGroup(
        ParserSymbolGroup group,
        string content, 
        ref ParserPosition position,
        out List<ParserResult>? result,
        out bool canFallback)
    {
        result = null;
        var startPosition = position;
        canFallback = true;

        foreach (var item in group)
        {
            var itemIsMatch = ParseItem(
                content, 
                ref position, 
                item, 
                out var itemResult, 
                out var itemCanFallback);

            if (itemResult != null)
            {
                result = result ?? new List<ParserResult>();
                result.AddRange(itemResult);
                canFallback = canFallback && itemCanFallback;
            }

            if (!itemIsMatch)
                return false;
        }

        return position.Index > startPosition.Index;
    }

    private bool ParseItem(
        string content, 
        ref ParserPosition position, 
        ParserItem item,
        out List<ParserResult>? result,
        out bool canFallback)
    {
        result = new List<ParserResult>();
        var positionResult = position;
        var resultCount = 0;
        canFallback = true;

        while (resultCount < item.MaxOccurs)
        {
            if (item is ParserRegex)
            {
                if (!ParseExpression(content, ref positionResult, (ParserRegex) item))
                    break;
            }
            else if (item is ParserReference)
            {
                var referenceIsMatch = ParseReference(
                    content, 
                    ref positionResult, 
                    (ParserReference) item, 
                    out var referenceResult);
                if (referenceResult != null)
                {
                    result.Add(referenceResult);
                    canFallback = canFallback && referenceResult.CanFallback;
                }

                if (!referenceIsMatch)
                    break;
            }
            else if (item is ParserEof)
            {
                if (!ParseEof(content, ref positionResult))
                    break;
            }
            else
                throw new InvalidOperationException();

            canFallback = canFallback && item.CanFallback;
            resultCount++;
        }
        if ((resultCount < item.MinOccurs) || (resultCount > item.MaxOccurs))
            return false;

        position = positionResult;
        return true;
    }

    private bool ParseExpression(
        string content, 
        ref ParserPosition position, 
        ParserRegex expression)
    {
        var matchResult = expression.MatchRegex.Match(content.Substring(position.Index));
        if (!matchResult.Success || string.IsNullOrEmpty(matchResult.Value))
            return false;

        position = position.Add(content, matchResult.Length);
        return true;
    }

    private bool ParseReference(
        string content, 
        ref ParserPosition position, 
        ParserReference reference,
        out ParserResult? result)
    {
        var referenceSymbol = this[reference.Name];

        result = ParseSymbol(referenceSymbol, content, position);
        if (result == null) 
            return false;

        position = result.EndPosition;
        return result.IsMatch;
    }

    private static bool ParseEof(
        string content, 
        ref ParserPosition position)
    {
        return position.Index >= content.Length;
    }
}