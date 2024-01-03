namespace Wxv.Parser;

internal class ParserValidator
{
    public ParserEngine Parser { get; private set; }

    public ParserValidator(ParserEngine parser)
    {
        Parser = parser;
        Validate();
    }

    private void Validate()
    {
        if (Parser.Count == 0)
            throw new InvalidDataException("No symbols defined");

        if (Parser.RootSymbol == null)
            throw new InvalidDataException("Root symbol not found");

        foreach (var symbol in Parser)
            ValidateSymbol(symbol);

        var symbolStack = new List<string>();
        ValidateRecursiveReferences(Parser.RootSymbol, symbolStack);

        symbolStack.Clear();
        if (!ValidateEof(Parser.RootSymbol, symbolStack))
            throw new InvalidDataException(@"No eof found");
    }

    private void ValidateSymbol(ParserSymbol symbol)
    {
        foreach (var group in symbol)
        {
            for (var itemIndex = 0; itemIndex < group.Count(); itemIndex++)
            {
                var item = group.ElementAt(itemIndex);

                if (item is ParserReference)
                    ValidateReference(symbol, (ParserReference)item);
                else if (item is ParserRegex)
                    ValidateRegex(symbol, (ParserRegex)item);
                else if (item is ParserEof)
                    ValidateEof(symbol, group, itemIndex);
            }
        }
    }

    private void ValidateReference(ParserSymbol symbol, ParserReference reference)
    {
        if (!Parser.ContainsSymbol(reference.Name))
            throw new InvalidDataException($@"Invalid reference : {symbol.Name}::{reference.Name}");
    }

    private void ValidateRegex(ParserSymbol symbol, ParserRegex regex)
    {
        try
        {
            // ReSharper disable once UnusedVariable
            var matchRegex = regex.MatchRegex;
        }
        catch
        {
            throw new InvalidDataException($@"Invalid expression : {symbol.Name}::{regex.Match}");
        }
        if (regex.MatchRegex.IsMatch(string.Empty))
            throw new InvalidDataException(
                $@"Invalid expression (cannot match empty string) : {symbol.Name}::{regex.Match}");
    }

    // ReSharper disable UnusedParameter.Local
    private void ValidateEof(ParserSymbol symbol, ParserSymbolGroup group, int itemIndex)
    {
        if (itemIndex != group.Count() - 1)
            throw new InvalidDataException($@"Invalid eof (must be at end of group) : {symbol.Name}");
    }
    // ReSharper enable UnusedParameter.Local

    private void ValidateRecursiveReferences(ParserSymbol symbol, List<string> symbolStack)
    {
        if (symbolStack.Contains(symbol.Name))
            throw new InvalidDataException(
                $@"Symbol contains a recursive reference.  Qualify with it an expresssion : {symbol.Name}");
        symbolStack.Add(symbol.Name);

        foreach (var group in symbol)
        {
            foreach (var item in group)
            {
                if (item is ParserRegex)
                {
                    break; // we qualify a recursive reference with an expression....so all is good
                }
                    
                if (item is ParserReference)
                {
                    var reference = (ParserReference)item;

                    // not qualified by an expression....so dig down
                    var referenceSymbol = Parser[reference.Name];
                    var referenceSymbolStack = new List<string>(symbolStack);
                    ValidateRecursiveReferences(referenceSymbol, referenceSymbolStack);
                }
            }
        }
    }

    private bool ValidateEof(ParserSymbol symbol, List<string> symbolStack)
    {
        if (symbolStack.Contains(symbol.Name))
            return false;
        symbolStack.Add(symbol.Name);

        foreach (var group in symbol)
        {
            foreach (var item in group)
            {
                if (item is ParserReference)
                {
                    var reference = (ParserReference)item;

                    // not qualified by an expression....so dig down
                    var referenceSymbol = Parser[reference.Name];
                    var referenceSymbolStack = new List<string>(symbolStack);
                    if (ValidateEof(referenceSymbol, referenceSymbolStack))
                        return true;
                }
                else if (item is ParserEof)
                {
                    return true;
                }
            }
        }

        return false;
    }
}