using System.Collections;

namespace Wxv.Nonograms.Core;

public interface IReadonlyGrid<T> : IEnumerable<T>
{
    int Width { get; }
    int Height { get; }
    T this[int x, int y] { get; }

    IEnumerable<T> GetRow(int rowIndex);
    IEnumerable<IEnumerable<T>> Rows { get; }
    IEnumerable<T> GetColumn(int columnIndex);
    IEnumerable<IEnumerable<T>> Columns { get; }
}

public interface IGrid<T>
{
    int Width { get; }
    int Height { get; }
    T this[int x, int y] { get; set; }

    IEnumerable<T> GetRow(int rowIndex);
    IEnumerable<IEnumerable<T>> Rows { get; }

    IEnumerable<T> GetColumn(int columnIndex);
    IEnumerable<IEnumerable<T>> Columns { get; }
}

public class Grid<T> : IReadonlyGrid<T>, IGrid<T>
{
    public int Width { get; } 
    public int Height { get; } 
    private T[,] Items { get; }
    
    public T this [int x, int y]
    {
        get => Items[x, y];
        set => Items[x, y] = value;
    }
    
    public Grid(int width, int height)
    {
        Width = width;
        Height = height;
        Items = new T[width, height];
    }
    
    public Grid(int width, int height, Func<int, int, T> getValues)
    {
        Width = width;
        Height = height;
        Items = new T[width, height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            Items[x, y] = getValues(x, y);
    }
    
    public IEnumerable<T> GetRow(int rowIndex) => Enumerable.Range(0, Width).Select(i => Items[i, rowIndex]);
    public IEnumerable<IEnumerable<T>> Rows => Enumerable.Range(0, Height).Select(GetRow);

    public IEnumerable<T> GetColumn(int columnIndex) => Enumerable.Range(0, Height).Select(i => Items[columnIndex, i]);
    public IEnumerable<IEnumerable<T>> Columns => Enumerable.Range(0, Width).Select(GetColumn);

    private IEnumerable<T> All()
    {
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            yield return Items[x, y];
    }
    
    public IEnumerator<T> GetEnumerator() => All().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => All().GetEnumerator();
}