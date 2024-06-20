using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.UiCommon.FormFlow.Tests;

public class JourneyInstanceIdTests
{
    [Fact]
    public void Create_MissingKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyDescriptor = new JourneyDescriptor(
            journeyName: "key",
            stateType: typeof(State),
            requestDataKeys: new[] { "id" },
            appendUniqueKey: true);

        var valueProvider = new DictionaryValueProvider(new Dictionary<string, string>());

        // Act
        var ex = Record.Exception(() => JourneyInstanceId.Create(journeyDescriptor, valueProvider));

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("Cannot resolve 'id' from request.", ex.Message);
    }

    [Fact]
    public void Create_NoDependentKeysWithoutUniqueKey_ReturnsCorrectInstance()
    {
        CreateReturnsExpectedInstance(
            requestDataKeys: Array.Empty<string>(),
            useUniqueKey: false,
            keys: null,
            expectedInstanceKeyCount: 0,
            assertions: instanceId => { },
            expectedSerializedValue: () => $"key");
    }

    [Fact]
    public void Create_NoDependentKeysWithUniqueKey_ReturnsCorrectInstance()
    {
        string? randomKey = default;

        CreateReturnsExpectedInstance(
            requestDataKeys: Array.Empty<string>(),
            useUniqueKey: true,
            keys: null,
            expectedInstanceKeyCount: 1,
            assertions: instanceId =>
            {
                randomKey = instanceId.Keys[Constants.UniqueKeyQueryParameterName];
                Assert.NotNull(randomKey);
            },
            expectedSerializedValue: () => $"key?ffiid={randomKey}");
    }

    [Fact]
    public void Create_DependentKeyFoundWithoutUniqueKey_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();

        CreateReturnsExpectedInstance(
            requestDataKeys: new[] { "id" },
            useUniqueKey: false,
            keys: new Dictionary<string, string>()
            {
                { "id", id }
            },
            expectedInstanceKeyCount: 1,
            assertions: instanceId =>
            {
                Assert.Equal(id, instanceId.Keys["id"]);
            },
            expectedSerializedValue: () => $"key?id={id}");
    }

    [Fact]
    public void Create_DependentKeyFoundWithUniqueKey_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();
        string? randomKey = default;

        CreateReturnsExpectedInstance(
            requestDataKeys: new[] { "id" },
            useUniqueKey: true,
            keys: new Dictionary<string, string>()
            {
                { "id", id }
            },
            expectedInstanceKeyCount: 2,
            assertions: instanceId =>
            {
                randomKey = instanceId.Keys[Constants.UniqueKeyQueryParameterName];
                Assert.Equal(id, instanceId.Keys["id"]);
            },
            expectedSerializedValue: () => $"key?id={id}&ffiid={randomKey}");
    }

    [Fact]
    public void Create_UniqueKeyAlreadyInKeys_ReturnsInstanceWithNewUniqueKey()
    {
        var currentRandomKey = Guid.NewGuid().ToString();
        string? newRandomKey = default;

        CreateReturnsExpectedInstance(
            requestDataKeys: Array.Empty<string>(),
            useUniqueKey: true,
            keys: new Dictionary<string, string>()
            {
                { Constants.UniqueKeyQueryParameterName, currentRandomKey }
            },
            expectedInstanceKeyCount: 1,
            assertions: instanceId =>
            {
                newRandomKey = instanceId.Keys[Constants.UniqueKeyQueryParameterName];
                Assert.NotNull(newRandomKey);
                Assert.NotEqual(currentRandomKey, newRandomKey);
            },
            expectedSerializedValue: () => $"key?ffiid={newRandomKey}");
    }

    [Fact]
    public void Create_OptionalDependentKeyFound_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();

        CreateReturnsExpectedInstance(
            requestDataKeys: new[] { "id?" },
            useUniqueKey: false,
            keys: new Dictionary<string, string>()
            {
                { "id", id }
            },
            expectedInstanceKeyCount: 1,
            assertions: instanceId =>
            {
                Assert.Equal(id, instanceId.Keys["id"]);
            },
            expectedSerializedValue: () => $"key?id={id}");
    }

    [Fact]
    public void Create_OptionalDependentKeyNotFound_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();

        CreateReturnsExpectedInstance(
            requestDataKeys: new[] { "id?" },
            useUniqueKey: false,
            keys: null,
            expectedInstanceKeyCount: 0,
            assertions: instanceId => { },
            expectedSerializedValue: () => $"key");
    }

    [Fact]
    public void TryResolve_MissingDependentRouteDataKey_ReturnsFalse()
    {
        // Arrange
        var journeyDescriptor = new JourneyDescriptor(
            journeyName: "key",
            stateType: typeof(State),
            requestDataKeys: new[] { "id" },
            appendUniqueKey: false);

        var valueProvider = new DictionaryValueProvider(new Dictionary<string, string>());

        // Act
        var result = JourneyInstanceId.TryResolve(journeyDescriptor, valueProvider, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryResolve_MissingUniqueKey_ReturnsFalse()
    {
        // Arrange
        var journeyDescriptor = new JourneyDescriptor(
            journeyName: "key",
            stateType: typeof(State),
            requestDataKeys: new[] { "id" },
            appendUniqueKey: true);

        var valueProvider = new DictionaryValueProvider(new Dictionary<string, string>()
        {
            { "id", Guid.NewGuid().ToString() }
        });

        // Act
        var result = JourneyInstanceId.TryResolve(journeyDescriptor, valueProvider, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryResolve_NoDependentKeysWithoutUniqueKey_ReturnsCorrectInstance()
    {
        TryResolveReturnsExpectedInstance(
            requestDataKeys: Array.Empty<string>(),
            useUniqueKey: false,
            keys: null,
            expectedInstanceKeyCount: 0,
            assertions: instanceId => { },
            expectedSerializableId: () => $"key");
    }

    [Fact]
    public void TryResolve_NoDependentKeysWithUniqueKey_ReturnsCorrectInstance()
    {
        var randomKey = Guid.NewGuid().ToString();

        TryResolveReturnsExpectedInstance(
            requestDataKeys: Array.Empty<string>(),
            useUniqueKey: true,
            keys: new Dictionary<string, string>()
            {
                { Constants.UniqueKeyQueryParameterName, randomKey }
            },
            expectedInstanceKeyCount: 1,
            assertions: instanceId =>
            {
                Assert.NotNull(randomKey);
            },
            expectedSerializableId: () => $"key?ffiid={randomKey}");
    }

    [Fact]
    public void TryResolve_DependentKeyFoundWithoutUniqueKey_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();

        TryResolveReturnsExpectedInstance(
            requestDataKeys: new[] { "id" },
            useUniqueKey: false,
            keys: new Dictionary<string, string>()
            {
                { "id", id }
            },
            expectedInstanceKeyCount: 1,
            assertions: instanceId =>
            {
                Assert.Equal(id, instanceId.Keys["id"]);
            },
            expectedSerializableId: () => $"key?id={id}");
    }

    [Fact]
    public void TryResolve_DependentKeyFoundWithUniqueKey_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();
        var randomKey = Guid.NewGuid().ToString();

        TryResolveReturnsExpectedInstance(
            requestDataKeys: new[] { "id" },
            useUniqueKey: true,
            keys: new Dictionary<string, string>()
            {
                { Constants.UniqueKeyQueryParameterName, randomKey },
                { "id", id }
            },
            expectedInstanceKeyCount: 2,
            assertions: instanceId =>
            {
                Assert.Equal(id, instanceId.Keys["id"]);
            },
            expectedSerializableId: () => $"key?id={id}&ffiid={randomKey}");
    }

    [Fact]
    public void TryResolve_OptionalDependentKeyFound_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();

        TryResolveReturnsExpectedInstance(
            requestDataKeys: new[] { "id?" },
            useUniqueKey: false,
            keys: new Dictionary<string, string>()
            {
                { "id", id }
            },
            expectedInstanceKeyCount: 1,
            assertions: instanceId =>
            {
                Assert.Equal(id, instanceId.Keys["id"]);
            },
            expectedSerializableId: () => $"key?id={id}");
    }

    [Fact]
    public void TryResolve_OptionalDependentKeyNotFound_ReturnsCorrectInstance()
    {
        var id = Guid.NewGuid().ToString();

        TryResolveReturnsExpectedInstance(
            requestDataKeys: new[] { "id?" },
            useUniqueKey: false,
            keys: null,
            expectedInstanceKeyCount: 0,
            assertions: instanceId => { },
            expectedSerializableId: () => $"key");
    }

    private void CreateReturnsExpectedInstance(
        IEnumerable<string> requestDataKeys,
        bool useUniqueKey,
        IDictionary<string, string>? keys,
        int expectedInstanceKeyCount,
        Action<JourneyInstanceId> assertions,
        Func<string> expectedSerializedValue)
    {
        // Arrange
        var journeyDescriptor = new JourneyDescriptor(
            journeyName: "key",
            stateType: typeof(State),
            requestDataKeys: requestDataKeys,
            appendUniqueKey: useUniqueKey);

        var valueProvider = new DictionaryValueProvider(keys ?? new Dictionary<string, string>());

        // Act
        var instanceId = JourneyInstanceId.Create(journeyDescriptor, valueProvider);

        // Assert
        Assert.Equal("key", instanceId.JourneyName);
        Assert.Equal(expectedInstanceKeyCount, instanceId.Keys.Count);
        assertions(instanceId);
        Assert.Equal(expectedSerializedValue(), instanceId.Serialize());
    }

    private void TryResolveReturnsExpectedInstance(
        IEnumerable<string> requestDataKeys,
        bool useUniqueKey,
        IDictionary<string, string>? keys,
        int expectedInstanceKeyCount,
        Action<JourneyInstanceId> assertions,
        Func<string> expectedSerializableId)
    {
        // Arrange
        var journeyDescriptor = new JourneyDescriptor(
            journeyName: "key",
            stateType: typeof(State),
            requestDataKeys: requestDataKeys,
            appendUniqueKey: useUniqueKey);

        var valueProvider = new DictionaryValueProvider(keys ?? new Dictionary<string, string>());

        // Act
        var result = JourneyInstanceId.TryResolve(journeyDescriptor, valueProvider, out var instanceId);

        // Assert
        Assert.True(result);
        Assert.Equal("key", instanceId.JourneyName);
        Assert.Equal(expectedInstanceKeyCount, instanceId.Keys.Count);
        assertions(instanceId);
        Assert.Equal(expectedSerializableId(), instanceId.ToString());
    }

    private class DictionaryValueProvider : IValueProvider
    {
        private readonly IDictionary<string, string> _values;

        public DictionaryValueProvider(IDictionary<string, string> values)
        {
            _values = values;
        }

        public bool ContainsPrefix(string prefix)
        {
            throw new NotImplementedException();
        }

        public ValueProviderResult GetValue(string key)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return new ValueProviderResult(value);
            }
            else
            {
                return ValueProviderResult.None;
            }
        }
    }

    private class State { }
}
