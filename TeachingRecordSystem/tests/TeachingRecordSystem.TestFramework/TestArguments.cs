using System.Collections;

namespace TeachingRecordSystem.TestFramework;

public abstract class TestArguments : IReadOnlyCollection<object?[]>
{
    private readonly List<object?[]> _data = new();

    public int Count => _data.Count;

    public IEnumerator<object?[]> GetEnumerator() => _data.GetEnumerator();

    protected void AddRow(params object?[] values)
    {
        _data.Add(values);
    }

    protected void AddRows(IEnumerable<object?[]> rows)
    {
        foreach (var row in rows)
        {
            AddRow(row);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class TestArguments<T> : TestArguments
{
    public TestArguments(IEnumerable<T> values) => AddRange(values.ToArray());

    public TestArguments(params T[] values) => AddRange(values);

    public void Add(T p) => AddRow(p);

    public void AddRange(params T[] values) =>
        AddRows(values.Select(x => new object?[] { x }));
}

public class TestArguments<T1, T2> : TestArguments
{
    public TestArguments(IEnumerable<(T1, T2)> values) => AddRange(values.ToArray());

    public TestArguments(params (T1, T2)[] values) => AddRange(values);

    public void Add(T1 p1, T2 p2) => AddRow(p1, p2);

    public void AddRange(params (T1 p1, T2 p2)[] values) =>
        AddRows(values.Select(x => new object?[] { x.p1, x.p2 }));
}

public class TestArguments<T1, T2, T3> : TestArguments
{
    public TestArguments(IEnumerable<(T1, T2, T3)> values) => AddRange(values.ToArray());

    public TestArguments(params (T1, T2, T3)[] values) => AddRange(values);

    public void Add(T1 p1, T2 p2, T3 p3) => AddRow(p1, p2, p3);

    public void AddRange(params (T1 p1, T2 p2, T3 p3)[] values) =>
        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3 }));
}

public class TestArguments<T1, T2, T3, T4> : TestArguments
{
    public TestArguments(IEnumerable<(T1, T2, T3, T4)> values) => AddRange(values.ToArray());

    public TestArguments(params (T1, T2, T3, T4)[] values) => AddRange(values);

    public void Add(T1 p1, T2 p2, T3 p3, T4 p4) => AddRow(p1, p2, p3, p4);

    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4)[] values) =>
        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4 }));
}

public class TestArguments<T1, T2, T3, T4, T5> : TestArguments
{
    public TestArguments(IEnumerable<(T1, T2, T3, T4, T5)> values) => AddRange(values.ToArray());

    public TestArguments(params (T1, T2, T3, T4, T5)[] values) => AddRange(values);

    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) => AddRow(p1, p2, p3, p4, p5);

    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)[] values) =>
        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5 }));
}

public class TestArguments<T1, T2, T3, T4, T5, T6> : TestArguments
{
    public TestArguments(IEnumerable<(T1, T2, T3, T4, T5, T6)> values) => AddRange(values.ToArray());

    public TestArguments(params (T1, T2, T3, T4, T5, T6)[] values) => AddRange(values);

    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) => AddRow(p1, p2, p3, p4, p5, p6);

    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)[] values) =>
        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6 }));
}

public class TestArguments<T1, T2, T3, T4, T5, T6, T7> : TestArguments
{
    public TestArguments(IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> values) => AddRange(values.ToArray());

    public TestArguments(params (T1, T2, T3, T4, T5, T6, T7)[] values) => AddRange(values);

    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) => AddRow(p1, p2, p3, p4, p5, p6, p7);

    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)[] values) =>
        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6, x.p7 }));
}

public class TestArguments<T1, T2, T3, T4, T5, T6, T7, T8> : TestArguments
{
    public TestArguments(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> values) => AddRange(values.ToArray());

    public TestArguments(params (T1, T2, T3, T4, T5, T6, T7, T8)[] values) => AddRange(values);

    public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => AddRow(p1, p2, p3, p4, p5, p6, p7, p8);

    public void AddRange(params (T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)[] values) =>
        AddRows(values.Select(x => new object?[] { x.p1, x.p2, x.p3, x.p4, x.p5, x.p6, x.p7, x.p8 }));
}
