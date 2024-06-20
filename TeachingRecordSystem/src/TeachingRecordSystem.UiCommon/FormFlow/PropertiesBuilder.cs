using System.Collections.ObjectModel;

namespace TeachingRecordSystem.UiCommon.FormFlow;

public class PropertiesBuilder
{
    private readonly Dictionary<object, object> _values;

    public PropertiesBuilder()
    {
        _values = [];
    }

    public PropertiesBuilder Add(object key, object value)
    {
        _values.Add(key, value);
        return this;
    }

    public IReadOnlyDictionary<object, object> Build() =>
        new ReadOnlyDictionary<object, object>(_values);

    public static IReadOnlyDictionary<object, object> CreateEmpty() =>
        new PropertiesBuilder().Build();
}
