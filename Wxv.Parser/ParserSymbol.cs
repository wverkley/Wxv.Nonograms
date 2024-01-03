using System.Collections;
using System.Text.RegularExpressions;

namespace Wxv.Parser;

public class ParserSymbol : IEnumerable<ParserSymbolGroup>
{
    public string Name { get; private set; }
    public bool IsRoot { get; private set; }
    public bool IsAnonymous { get; private set; }

    private readonly List<ParserSymbolGroup> _groups = new();

    internal ParserSymbol(string name, bool isRoot = false, bool isAnonymous = false)
    {
        if (string.IsNullOrWhiteSpace(name) || !ParserConstants.ValidName.IsMatch(name))
            throw new ArgumentException("Invalid symbol name: {0}", name);

        if (isRoot && isAnonymous)
            throw new ArgumentException("Symbol cannot be root and anonymous: {0}", name);

        Name = name;
        IsRoot = isRoot;
        IsAnonymous = isAnonymous;
    }

    internal void AddGroup(ParserSymbolGroup group)
    {
        _groups.Add(group);
    }

    public override string ToString()
    {
        return Name + (IsRoot ? "*" : "");
    }

    public IEnumerator<ParserSymbolGroup> GetEnumerator()
    {
        return _groups.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _groups.GetEnumerator();
    }
}

public class ParserSymbolGroup : IEnumerable<ParserItem>
{
    private readonly List<ParserItem> _items = new();

    internal ParserSymbolGroup()
    {
    }

    internal void AddItem(ParserItem item)
    {
        _items.Add(item);
    }

    public IEnumerator<ParserItem> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }
}

public abstract class ParserItem
{
    public bool CanFallback;
    public int MinOccurs { get; private set; }
    public int MaxOccurs { get; private set; }

    internal ParserItem(int minOccurs = 1, int maxOccurs = 1, bool canFallback = true)
    {
        if (minOccurs < 0)
            throw new ArgumentException(@"Invalid minOccurs value");
        if (maxOccurs < minOccurs)
            throw new ArgumentException(@"Invalid maxOccurs value");

        MinOccurs = minOccurs;
        MaxOccurs = maxOccurs;
        CanFallback = canFallback;
    }
}

public class ParserReference : ParserItem
{
    public string Name { get; private set; }

    internal ParserReference(string name, int minOccurs = 1, int maxOccurs = 1, bool canFallback = true)
        : base(minOccurs, maxOccurs, canFallback)
    {
        if (string.IsNullOrWhiteSpace(name) || !ParserConstants.ValidName.IsMatch(name))
            throw new ArgumentException("Invalid symbol name: {0}", name);

        Name = name;
    }
}

public class ParserRegex : ParserItem
{
    public string Match { get; }

    private Regex? _matchRegex;
    public Regex MatchRegex => _matchRegex ??= new Regex("^(" + Match +")", RegexOptions.CultureInvariant | RegexOptions.Singleline);

    internal ParserRegex(string match, int minOccurs = 1, int maxOccurs = 1, bool canFallback = true)
        : base(minOccurs, maxOccurs, canFallback)
    {
        Match = match;
    }
}

public class ParserEof : ParserItem
{
    internal ParserEof(bool canFallback = true)
        : base(1, 1, canFallback)
    {
        CanFallback = canFallback;
    }

}