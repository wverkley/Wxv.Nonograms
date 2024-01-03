namespace Wxv.Core;


public class ConsecutiveResult<T>
{
	public int Index { get; }
	public int Count { get; }
	public T Value { get; }
	public bool IsFirst { get; internal set; }
	public ConsecutiveResult<T>? Previous { get; internal set; }
	public bool IsLast { get; internal set; }
	public ConsecutiveResult<T>? Next { get; internal set; }

	public ConsecutiveResult(int index, int count, T value)
	{
		Index = index;
		Count = count;
		Value = value;
	}

	public override string ToString() => $"{Index} : {Count} : {Value}";
}

public static class ConsecutiveExtensions
{
	public static IEnumerable<ConsecutiveResult<T>> GetConsecutive<T>(this IEnumerable<T> items)
	{
		var count = 0;
		var firstValue = default(T)!;
		var firstIndex = 0;
		var index = 0;
		var result = new List<ConsecutiveResult<T>>();
		foreach(var item in items)
		{
			if (count == 0)
			{
				firstValue = item;
				firstIndex = index;
				count++;
			}
			else if (firstValue!.Equals(item))
			{
				count++;
			}
			else
			{
				result.Add(new ConsecutiveResult<T>(firstIndex, count, firstValue));
				firstValue = item;
				firstIndex = index;
				count = 1;
			}
				
			index++;
		}
		if (count > 0)
			result.Add(new ConsecutiveResult<T>(firstIndex, count, firstValue));

		if (!result.Any())
			return result;

		for (var i = 0; i < result.Count; i++)
		{
			result[i].IsFirst = i == 0;
			result[i].Previous = !result[i].IsFirst ? result[i - 1] : null;
			result[i].IsLast = i == result.Count - 1;
			result[i].Next = !result[i].IsLast ? result[i + 1] : null;
		}
		return result;
	}

	public static int GetMaxConsecutive<T>(this IEnumerable<T> items, T value)
		where T : IEquatable<T>
		=> items
			.GetConsecutive()
			.Where(kv => kv.Value.Equals(value))
			.Select(kv => kv.Count)
			.Max();

	public static bool GetConsecutiveAt<T>(
		this IEnumerable<ConsecutiveResult<T>> items,
		int index,
		out ConsecutiveResult<T>? result)
		where T : IEquatable<T>
	{
		result = null;
		if (index < 0 || !items.Any())
			return false;

		foreach (var item in items)
		{
			if (item.Index <= index && item.Index + item.Count > index)
			{
				result = item;
				return true;
			}
		}

		return false;
	}

	public static int GetConsecutiveCountAt<T>(
		this IEnumerable<ConsecutiveResult<T>> items,
		int index)
		where T : IEquatable<T>
		=> GetConsecutiveAt(items, index, out var result)
			? result!.Count - (index - result.Index)
			: 0;

	public static T? GetConsecutiveValueAt<T>(
		this IEnumerable<ConsecutiveResult<T>> items,
		int index)
		where T : IEquatable<T>
		=> GetConsecutiveAt(items, index, out var result)
			? result!.Value
			: default(T);

}