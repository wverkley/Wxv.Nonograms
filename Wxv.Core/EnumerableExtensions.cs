namespace Wxv.Core;

public static class EnumerableExtensions
{
	private static readonly Lazy<Random> LazyRandom = new(() => new Random());

	/// <summary>
	/// Get a random item from an enumerable
	/// </summary>
	public static T? GetRandomItem<T>(this IEnumerable<T> values, Random? random = null)
	{
		if (!values.Any())
			return default;

		var valueList = values.ToList();
		var index = (random ?? LazyRandom.Value).Next(0, valueList.Count);
		return valueList[index];
	}

	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T> action)
	{
		foreach (var value in values)
			if (value != null)
				action(value);
		return values;
	}

	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> values, Action<T, int> action)
	{
		var i = 0;
		foreach (var value in values)
			if (value != null)
				action(value, i++);
		return values;
	}

	public static string JoinString<T>(this IEnumerable<T> values)
	{
		using var writer = new StringWriter();
		foreach (var value in values)
			writer.Write(value?.ToString());
		return writer.ToString();
	}

	public static string JoinString<T>(this IEnumerable<T> values, string separator)
		=> string.Join(separator, values.Select(value => value?.ToString()));

}