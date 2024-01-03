using Xunit.Abstractions;

namespace Wxv.Core.Tests.String;

public class StringRangeTests
{
    private ITestOutputHelper Logger { get; }

    public StringRangeTests(ITestOutputHelper logger) => Logger = logger;

    private void Test(string name, string input, string expected, Func<StringRange, StringRange> func)
    {
        Logger.WriteLine($"{name}: {input} -> {expected}");
        Assert.Equal(expected, func(input));
    }

    [Fact] public void StartsWith() => Test("StartsWith", "Hello, World", ", World", s => s.StartsWith(","));
    [Fact] public void StartsWith_Begin() => Test("StartsWith", "Hello, World", "Hello, World", s => s.StartsWith("Hello"));
    [Fact] public void StartsWith_End() => Test("StartsWith", "Hello, World", "World", s => s.StartsWith("World"));
    [Fact] public void StartsAfter() => Test("StartsAfter", "Hello, World", " World", s => s.StartsAfter(","));
    [Fact] public void StartsAfter_Begin() => Test("StartsWith", "Hello, World", ", World", s => s.StartsAfter("Hello"));
    [Fact] public void StartsAfter_End() => Test("StartsWith", "Hello, World", "", s => s.StartsAfter("World"));
    [Fact] public void EndsBefore() => Test("EndsBefore", "Hello, World", "Hello", s => s.EndsBefore(","));
    [Fact] public void EndsWith() => Test("EndsWith", "Hello, World", "Hello,", s => s.EndsWith(","));
    [Fact] public void Within() => Test("Within", "Hello, World", ", ", s => s.Within("Hello", "World"));
    [Fact] public void Between() => Test("Between", "Hello, World", "Hello, World", s => s.Between("Hello", "World"));
        
    private void Test(string name, string input, string expected, Func<StringRange, IEnumerable<StringRange>> func)
    {
        Logger.WriteLine($"{name}: {input} -> {expected}");
        Assert.Equal(expected, string.Join("|", func(input)));
    }

    [Fact] public void Split() => Test("Split", "Hello, World", "Hello| World", s => s.Split(","));
    [Fact] public void Split_NoMatch() => Test("Split", "Hello, World", "Hello, World", s => s.Split("|"));
        
}