using System.Collections;

namespace Wxv.Parser;

public class ParserResult : IEnumerable<ParserResult>
{
    private static readonly IEnumerable<ParserResult> Empty = new List<ParserResult>();

    public ParserSymbol Symbol { get; internal init; }
    public string Content { get; internal init; }
    public ParserPosition StartPosition { get; internal init; }
    public ParserPosition EndPosition { get; internal init; }
    internal bool CanFallback { get; init; }
    public bool IsMatch { get; internal init; }
    internal IEnumerable<ParserResult>? Children { get; set; }

    public string Name => Symbol.Name;
    public bool IsRoot => Symbol.IsRoot;
    public bool IsAnonymous => Symbol.IsAnonymous;
    public int Length => EndPosition.Index - StartPosition.Index;
    public string Value => Content.Substring(StartPosition.Index, Length);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal ParserResult() {}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public void ToString(StringWriter writer, int indent)
    {
        writer.Write("{0}{1}{2} {3}-{4}{5}", 
            new string(' ', indent * 2), 
            IsAnonymous ? "-" : "", 
            Name, 
            StartPosition, 
            EndPosition, 
            IsMatch ? "*" : "");
        if (!this.Any())
            writer.Write(": "+ Value);
        writer.WriteLine();

        foreach (var child in this)
            child.ToString(writer, indent + 1);
    }

    public override string ToString()
    {
        using var writer = new StringWriter();
        ToString(writer, 0);
        return writer.ToString();
    }

    public IEnumerator<ParserResult> GetEnumerator()
    {
        return Children == null ? Empty.GetEnumerator() : Children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Children == null ? Empty.GetEnumerator() : Children.GetEnumerator();
    }

    public void RemoveAnonymous()
    {
        RemoveChildren(child => child.IsAnonymous);
    }

    public void RemoveChildren(Func<ParserResult, bool> predicate)
    {
        foreach (var child in this)
            child.RemoveChildren(predicate);

        if (!this.Any(predicate))
            return;

        // we have matching children.....copy down the grandchildren of to this instance
        var newChildren = new List<ParserResult>();
        foreach (var child in this)
        {
            if (predicate(child))
            {
                if (child.Children != null)
                    newChildren.AddRange(child.Children);
            }
            else
                newChildren.Add(child);
        }
        Children = newChildren;
    }
}