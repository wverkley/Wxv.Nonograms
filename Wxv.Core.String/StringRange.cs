using System.Collections;
using System.Runtime.Serialization;

namespace Wxv.Core;

public enum StringRangeCategory
{
    Root,
    Split,
    StarsWith,
    StartsAfter,
    EndsWith,
    EndsBefore,
    Within,
    Between,
    After,
    Before,
}

public class StringRange
{
    public static StringRange Null { get; } = new StringRange(null, string.Empty, -1, -1);
    public static IEnumerable<StringRange> Empty { get; } = new StringRange[] { };
        
    [IgnoreDataMember]
    public StringRange? Parent { get; private set; }
    [IgnoreDataMember]
    public StringRange Root => Parent != null ? Parent.Root : Root;
        
    private List<StringRange> ChildrenList { get; } = new();
    public IEnumerable Children => ChildrenList;
        
    [IgnoreDataMember]
    public string Source { get; }
    public int From { get; }
    public int To { get; }
    public string Value => IsNull() ? string.Empty : Source.Substring(From, To - From);
    public StringRangeCategory Category { get; }
    public IReadOnlyDictionary<string, string>? Parameters { get; }

    private StringRange(StringRange? parent, string source, int from, int to, StringRangeCategory category = StringRangeCategory.Root, IReadOnlyDictionary<string, string>? parameters = null)
    {
        Parent = parent;
        parent?.ChildrenList.Add(this);
        Source = source;
        From = from;
        To = to;
        Category = category;
        Parameters = parameters;
    }
    public StringRange(string source) : this(null, source, 0, source.Length) 
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
    }

    public bool IsNull() { return From == -1; }
    public bool IsNotNull() { return From != -1; }

    private IEnumerable<StringRange> All(Func<StringRange, StringRange> func, Func<StringRange, StringRange>? nextFunc = null)
    {
        var result = func(this);
        while (!result.IsNull())
        {
            yield return result;

            if (nextFunc != null)
                result = nextFunc(result);
                
            if (!result.IsNull())
                result = func(result);
        }
    }

    public IEnumerable<StringRange> Split(string separatorTag)
    {
        if (string.IsNullOrEmpty(separatorTag)) 
            throw new ArgumentNullException(nameof(separatorTag));
        if (IsNull()) 
            yield return Null;

        var from = From;
        while (from < To)
        {
            var separatorTagIndex = Source.IndexOf(separatorTag, from, StringComparison.Ordinal);
            if (separatorTagIndex < 0 || separatorTagIndex >= To)
            {
                yield return new StringRange(this, Source, from, To,
                    StringRangeCategory.Split,
                    new Dictionary<string, string>
                    {
                        {nameof(separatorTag), separatorTag}
                    });
                from = To;
            }
            else
            {
                yield return new StringRange(this, Source, from, separatorTagIndex,
                    StringRangeCategory.Split,
                    new Dictionary<string, string>
                    {
                        {nameof(separatorTag), separatorTag}
                    });
                from = separatorTagIndex + separatorTag.Length;
            }
        }
    }
        
    public StringRange StartsWith(string startTag)
    {
        if (string.IsNullOrEmpty(startTag)) 
            throw new ArgumentNullException(nameof(startTag));
        if (IsNull()) 
            return Null;

        var startTagIndex = Source.IndexOf(startTag, From, StringComparison.Ordinal);
        if (startTagIndex < 0 || startTagIndex >= To) 
            return Null;

        return new StringRange(this, Source, startTagIndex, To,
            StringRangeCategory.StarsWith,
            new Dictionary<string, string>
            {
                { nameof(startTag), startTag }
            });
    }

    public StringRange StartsAfter(string startTag)
    {
        if (string.IsNullOrEmpty(startTag)) 
            throw new ArgumentNullException(nameof(startTag));
        if (IsNull()) 
            return Null;

        var startTagIndex = Source.IndexOf(startTag, From, StringComparison.Ordinal);
        if (startTagIndex < 0 || startTagIndex >= To) 
            return Null;

        return new StringRange(this, Source, startTagIndex + startTag.Length, To,
            StringRangeCategory.StartsAfter,
            new Dictionary<string, string>
            {
                { nameof(startTag), startTag }
            });
    }
    public IEnumerable<StringRange> AllStartsAfter(string startTag) => All(s => s.StartsAfter(startTag));

    public StringRange EndsWith(string endTag)
    {
        if (string.IsNullOrEmpty(endTag)) 
            throw new ArgumentNullException(nameof(endTag));
        if (IsNull()) 
            return Null;

        var endTagIndex = Source.IndexOf(endTag, From, StringComparison.Ordinal);
        if (endTagIndex < 0 || (endTagIndex + endTag.Length) >= To) 
            return Null;

        return new StringRange(this, Source, From, endTagIndex + endTag.Length,
            StringRangeCategory.EndsWith,
            new Dictionary<string, string>
            {
                { nameof(endTag), endTag }
            });
    }
    public IEnumerable<StringRange> AllEndsWith(string endTag) => All(
        s => s.EndsWith(endTag), 
        s => s.After());

    public StringRange EndsBefore(string endTag)
    {
        if (string.IsNullOrEmpty(endTag)) 
            throw new ArgumentNullException(nameof(endTag));
        if (IsNull()) 
            return Null;

        var endTagIndex = Source.IndexOf(endTag, From, StringComparison.Ordinal);
        if (endTagIndex < 0 || endTagIndex >= To) 
            return Null;

        return new StringRange(this, Source, From, endTagIndex,
            StringRangeCategory.EndsBefore,
            new Dictionary<string, string>
            {
                { nameof(endTag), endTag }
            });
    }

    private StringRange BetweenOrWithin(string startTag, string endTag, StringRangeCategory category)
    {
        if (string.IsNullOrEmpty(startTag)) throw new ArgumentNullException(nameof(startTag));
        if (string.IsNullOrEmpty(endTag)) throw new ArgumentNullException(nameof(endTag));

        if (IsNull()) return Null;

        var startTagIndex = Source.IndexOf(startTag, From, StringComparison.Ordinal);
        if (startTagIndex < 0 || startTagIndex >= To) return Null;

        var endTagIndex = Source.IndexOf(endTag, startTagIndex + startTag.Length, StringComparison.Ordinal);
        if (endTagIndex < 0 || (endTagIndex + endTag.Length) > To) return Null;

        return new StringRange(
            this, 
            Source, 
            category == StringRangeCategory.Between ? startTagIndex : startTagIndex + startTag.Length, 
            category == StringRangeCategory.Between ? endTagIndex + endTag.Length : endTagIndex, 
            Category,
            new Dictionary<string, string>
            {
                { nameof(startTag), startTag },
                { nameof(endTag), endTag }
            });
    }

    public StringRange Between(string startTag, string endTag) => BetweenOrWithin(startTag, endTag, StringRangeCategory.Between);
    public IEnumerable<StringRange> AllBetween(string startTag, string endTag) => All(
        s => s.Between(startTag, endTag),
        s => s.After());

    public StringRange Within(string startTag, string endTag) => BetweenOrWithin(startTag, endTag, StringRangeCategory.Within);
    public IEnumerable<StringRange> AllWithin(string startTag, string endTag) => All(
        s => s.Within(startTag, endTag),
        s => s.After());

    public StringRange Before()
    {
        if (IsNull() || Parent == null || From <= Parent.From)
            return Null;
        return new StringRange(Parent.Parent, Source, Parent.From, From, StringRangeCategory.Before);
    }

    public StringRange After()
    {
        if (IsNull() || Parent == null || To >= Parent.To)
            return Null;
        return new StringRange(Parent.Parent, Source, To, Parent.To, StringRangeCategory.After);
    }

    public string ToVerboseString()
    {
        if (IsNull())
            return string.Empty;

        using var writer = new StringWriter();
        writer.WriteLine($"{nameof(Category)}: {Category}");
        writer.Write($"{nameof(Parent)}: "); writer.WriteLine(Parent != null ? "set" : "null");
        writer.WriteLine($"{nameof(From)}: {From}");
        writer.WriteLine($"{nameof(To)}: {To}");
        writer.WriteLine($"{nameof(Children)}: ({ChildrenList.Count})");
        writer.WriteLine($"{nameof(Parameters)}: ({Parameters?.Count})");
        if (Parameters != null)
            foreach (var kv in Parameters)
                writer.WriteLine($"  {kv.Key}: {kv.Value}");
        writer.Write(Value);
        return writer.ToString();
    }

    public override string ToString() => Value;

    public static implicit operator StringRange(string v) => new StringRange(v);
    public static implicit operator string(StringRange v) => v.Value;
        
}