using Xunit.Abstractions;

namespace Wxv.Core.Tests;

public class ConsecutiveExtensionsTests
{
    private ITestOutputHelper Logger { get; }

    public ConsecutiveExtensionsTests(ITestOutputHelper logger) => Logger = logger;

    [Fact]
    public void Empty() => Assert.Empty(string.Empty.GetConsecutive());

    [Fact]
    public void Single()
    {
        var cosecutives = "111".GetConsecutive();
        Assert.Single(cosecutives);
        Assert.Equal(3, cosecutives.First().Count);
        Assert.Equal('1', cosecutives.First().Value);
        Assert.Equal(0, cosecutives.First().Index);
        Assert.True(cosecutives.First().IsFirst);
        Assert.Null(cosecutives.First().Previous);
        Assert.True(cosecutives.First().IsLast);
        Assert.Null(cosecutives.First().Next);
    }

    [Fact]
    public void Complex()
    {
        var cosecutives = "1223334444".GetConsecutive().ToList();
        Assert.Equal(4, cosecutives.Count());
        Assert.True(cosecutives.Select(c => c.Value).SequenceEqual(new[] { '1', '2', '3', '4' }));
        Assert.True(cosecutives.Select(c => c.Index).SequenceEqual(new[] { 0, 1, 3, 6 }));
        Assert.True(cosecutives.Select(c => c.Count).SequenceEqual(new[] { 1, 2, 3, 4 }));
        Assert.True(cosecutives.Select(c => c.IsFirst).SequenceEqual(new[] { true, false, false, false }));
        Assert.True(cosecutives.Select(c => c.Previous).SequenceEqual(new[] { null, cosecutives[0], cosecutives[1], cosecutives[2] }));
        Assert.True(cosecutives.Select(c => c.IsLast).SequenceEqual(new[] { false, false, false, true }));
        Assert.True(cosecutives.Select(c => c.Next).SequenceEqual(new[] { cosecutives[1], cosecutives[2], cosecutives[3], null }));
    }
    
    
}