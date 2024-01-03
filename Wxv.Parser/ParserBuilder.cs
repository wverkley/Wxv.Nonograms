namespace Wxv.Parser;

public class ParserBuilder
{
    private readonly ParserEngine _parser = new();
    private ParserSymbol? _currentSymbol;
    private ParserSymbolGroup? _currentGroup;

    public ParserBuilder Symbol(string name, bool isRoot = false, bool isAnonymous = false)
    {
        _currentSymbol = new ParserSymbol(name, isRoot, isAnonymous);
        _currentGroup = new ParserSymbolGroup();
        _currentSymbol.AddGroup(_currentGroup);
        _parser.AddSymbol(_currentSymbol);
        return this;
    }

    public ParserBuilder Or()
    {
        if (_currentSymbol == null)
            throw new InvalidOperationException("No symbol");

        _currentGroup = new ParserSymbolGroup();
        _currentSymbol.AddGroup(_currentGroup);
        return this;
    }

    public ParserBuilder Regex(string match, int minOccurs = 1, int maxOccurs = 1, bool canFallback = true)
    {
        if (_currentSymbol == null || _currentGroup == null)
            throw new InvalidOperationException("No symbol");

        var parserRegex = new ParserRegex(match, minOccurs, maxOccurs, canFallback);
        _currentGroup.AddItem(parserRegex);
        return this;
    }

    public ParserBuilder Reference(string name, int minOccurs = 1, int maxOccurs = 1, bool canFallback = true)
    {
        if (_currentSymbol == null || _currentGroup == null)
            throw new InvalidOperationException("No symbol");

        var parserReference = new ParserReference(name, minOccurs, maxOccurs, canFallback);
        _currentGroup.AddItem(parserReference);
        return this;
    }

    public ParserBuilder Eof(bool canFallback = true)
    {
        if (_currentSymbol == null || _currentGroup == null)
            throw new InvalidOperationException("No symbol");

        var parserEof = new ParserEof(canFallback);
        _currentGroup.AddItem(parserEof);
        return this;
    }

    public ParserEngine Build()
    {
        _ = new ParserValidator(_parser);
        return _parser;
    }
}