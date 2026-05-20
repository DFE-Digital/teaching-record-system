using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.WebCommon.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.WebCommon.Tests.FormFlow;

public class JourneyInstanceProviderTests
{
    private readonly IOptions<FormFlowOptions> _options;

    public JourneyInstanceProviderTests()
    {
        var options = new FormFlowOptions();
        _options = Options.Create(options);
    }

    [Fact]
    public async Task CreateInstanceAsync_ActionHasNoMetadata_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        var actionDescriptor = new ActionDescriptor();

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.CreateInstanceAsync(actionContext, id => state));

        // Act & Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("No journey metadata found on action.", ex.Message);
    }

    [Fact]
    public async Task CreateInstanceAsync_StateTypeIsIncompatible_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();
        var descriptorStateType = typeof(OtherTestState);

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, descriptorStateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.CreateInstanceAsync(actionContext, _ => state));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"{typeof(TestState).FullName} is not compatible with the journey's state type ({typeof(OtherTestState).FullName}).", ex.Message);
    }

    [Fact]
    public async Task CreateInstanceAsync_InstanceAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        var id = 42;
        var subid = 69;

        var routeValues = new RouteValueDictionary()
        {
            { "id", 42 },
            { "subid", 69 }
        };

        var instanceId = new JourneyInstanceId(journeyName, new Dictionary<string, StringValues>()
        {
            { "id", id.ToString() },
            { "subid", subid.ToString() }
        });

        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.GetRouteData().Values.AddRange(routeValues);

        var routeData = new RouteData(routeValues);

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, requestDataKeys: ["id", "subid"], appendUniqueKey: false));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.CreateInstanceAsync(actionContext, _ => state));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("Instance already exists with this ID.", ex.Message);
    }

    [Fact]
    public async Task CreateInstanceAsync_CreatesInstanceInStateStore()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.CreateInstanceAsync(
                It.IsAny<JourneyInstanceId>(),  // FIXME
                stateType,
                state))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state))
            .Verifiable();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.CreateInstanceAsync(actionContext, _ => state);

        // Assert
        stateProvider.Verify();
        Assert.NotNull(result);
        Assert.Equal(journeyName, result.JourneyName);
        Assert.Equal(instanceId, result.InstanceId);
        Assert.Equal(stateType, result.StateType);
        Assert.Same(state, result.State);
        Assert.False(result.Completed);
        Assert.False(result.Deleted);
    }

    [Fact]
    public async Task CreateInstanceOfT_StateTypeIsIncompatible_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        TestState state = new TestState();
        var descriptorStateType = typeof(OtherTestState);

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, descriptorStateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.CreateInstanceAsync(actionContext, _ => state));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"{typeof(TestState).FullName} is not compatible with the journey's state type ({typeof(OtherTestState).FullName}).", ex.Message);
    }

    [Fact]
    public async Task GetInstanceAsync_ActionHasNoMetadata_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        var actionDescriptor = new ActionDescriptor();

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.GetInstanceAsync(actionContext));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("No journey metadata found on action.", ex.Message);
    }

    [Fact]
    public async Task GetInstanceAsync_InstanceDoesNotExist_ReturnsNull()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync((JourneyInstance?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.GetInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInstanceAsync_InstanceDoesExist_ReturnsInstance()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.GetInstanceAsync(actionContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(journeyName, result!.JourneyName);
        Assert.Equal(instanceId, result.InstanceId);
        Assert.Equal(stateType, result.StateType);
        Assert.Same(state, result.State);
        Assert.False(result.Completed);
        Assert.False(result.Deleted);
    }

    [Fact]
    public async Task GetInstanceOfT_StateTypeIsIncompatible_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.GetInstanceAsync<OtherTestState>(actionContext));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"{typeof(OtherTestState).FullName} is not compatible with the journey's state type ({typeof(TestState).FullName}).", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateInstanceAsync_ActionHasNoMetadata_ThrowsInvalidOperationException()
    {
        // Arrange
        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.GetOrCreateInstanceAsync(actionContext, _ => new TestState()));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("No journey metadata found on action.", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateInstanceAsync_InstanceDoesNotExist_CreatesInstanceInStateStore()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync((JourneyInstance?)null);
        stateProvider
            .Setup(mock => mock.CreateInstanceAsync(
                It.IsAny<JourneyInstanceId>(),
                stateType,
                It.IsAny<object>()))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state))
            .Verifiable();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.GetOrCreateInstanceAsync(actionContext, _ => new TestState());

        // Assert
        stateProvider.Verify();
        Assert.NotNull(result);
        Assert.Equal(journeyName, result.JourneyName);
        Assert.Equal(instanceId, result.InstanceId);
        Assert.Equal(stateType, result.StateType);
        Assert.Same(state, result.State);
        Assert.False(result.Completed);
        Assert.False(result.Deleted);
    }

    [Fact]
    public async Task GetOrCreateInstanceAsync_CreateStateStateTypeIsIncompatible_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync((JourneyInstance?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.GetOrCreateInstanceAsync(actionContext, _ => (object)new OtherTestState()));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"{typeof(OtherTestState).FullName} is not compatible with the journey's state type ({typeof(TestState).FullName}).", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateInstanceAsync_InstanceDoesExist_ReturnsExistingInstance()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object originalState = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    originalState));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        var executedStateFactory = false;

        // Act
        var result = await instanceProvider.GetOrCreateInstanceAsync(
            actionContext,
            _ =>
            {
                executedStateFactory = true;
                return new TestState();
            });

        // Assert
        Assert.False(executedStateFactory);
        Assert.NotNull(result);
        Assert.Equal(journeyName, result.JourneyName);
        Assert.Equal(instanceId, result.InstanceId);
        Assert.Equal(stateType, result.StateType);
        Assert.Same(originalState, result.State);
        Assert.False(result.Completed);
        Assert.False(result.Deleted);
    }

    [Fact]
    public async Task GetOrCreateInstanceOfT_CreateStateStateTypeIsIncompatible_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync((JourneyInstance?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(
            () => instanceProvider.GetOrCreateInstanceAsync(actionContext, _ => new OtherTestState()));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"{typeof(OtherTestState).FullName} is not compatible with the journey's state type ({typeof(TestState).FullName}).", ex.Message);
    }

    [Fact]
    public async Task GetOrCreateInstanceFfT_RequestedStateTypeIsIncompatible_ThrowsInvalidOperationException()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        object state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(mock => mock.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var ex = await Record.ExceptionAsync(() => instanceProvider.GetOrCreateInstanceAsync(actionContext, _ => new OtherTestState()));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal($"{typeof(OtherTestState).FullName} is not compatible with the journey's state type ({typeof(TestState).FullName}).", ex.Message);
    }

    [Fact]
    public void IsCurrentInstance_InstanceMatches_ReturnsTrue()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        var otherInstanceId = new JourneyInstanceId(
            journeyName,
            new Dictionary<string, StringValues>()
            {
                { Constants.UniqueKeyQueryParameterName, uniqueKey }
            });

        // Act
        var result = instanceProvider.IsCurrentInstance(actionContext, otherInstanceId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsCurrentInstance_NoCurrentInstance_ReturnsFalse()
    {
        // Arrange
        var journeyName = "test-flow";
        CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        var otherInstanceId = new JourneyInstanceId("another-id", new Dictionary<string, StringValues>());

        // Act
        var result = instanceProvider.IsCurrentInstance(actionContext, otherInstanceId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCurrentInstance_DifferentInstanceToCurrent_ReturnsFalse()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        var otherInstanceId = new JourneyInstanceId("another-id", new Dictionary<string, StringValues>());

        // Act
        var result = instanceProvider.IsCurrentInstance(actionContext, otherInstanceId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_ActionHasNoMetadata_ReturnsNull()
    {
        // Arrange
        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ActionDescriptor();

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_CannotExtractIdForRouteValues_ReturnsNull()
    {
        // Arrange
        var journeyName = "test-flow";
        var id = 42;
        var subid = 69;

        var routeValues = new RouteValueDictionary()
        {
            { "id", 42 },
            { "subid", 69 }
        };

        var instanceId = new JourneyInstanceId(journeyName, new Dictionary<string, StringValues>()
        {
            { "id", id.ToString() },
            { "subid", subid.ToString() }
        });

        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(
                journeyName,
                stateType,
                requestDataKeys: ["id", "subid"],
                appendUniqueKey: false));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_CannotExtractIdForRandomId_ReturnsNull()
    {
        // Arrange
        var journeyName = "test-flow";
        var stateType = typeof(TestState);

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var httpContext = new DefaultHttpContext();

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_InstanceDoesNotExistInStateStore_ReturnsNull()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync((JourneyInstance?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_MismatchingJourneyNames_ReturnsNull()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();
        var otherJourneyName = "another-name";

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(otherJourneyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(otherJourneyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_MismatchingStateType_ReturnsNull()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();
        var descriptorStateType = typeof(OtherTestState);

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, descriptorStateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_InstanceExistsForRandomId_ReturnsInstance()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(journeyName, result!.JourneyName);
        Assert.Equal(instanceId, result.InstanceId);
        Assert.Equal(stateType, result.StateType);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_InstanceExistsForRouteValues_ReturnsInstance()
    {
        // Arrange
        var journeyName = "test-flow";
        var id = 42;
        var subid = 69;

        var routeValues = new RouteValueDictionary()
        {
            { "id", id },
            { "subid", subid }
        };

        var instanceId = new JourneyInstanceId(journeyName, new Dictionary<string, StringValues>()
        {
            { "id", id.ToString() },
            { "subid", subid.ToString() }
        });

        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/foo/42/69";
        httpContext.GetRouteData().Values.AddRange(routeValues);

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(
                journeyName,
                stateType,
                requestDataKeys: ["id", "subid"],
                appendUniqueKey: false));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(journeyName, result!.JourneyName);
        Assert.Equal(instanceId, result.InstanceId);
        Assert.Equal(stateType, result.StateType);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_InstanceIsDeleted_ReturnsFalse()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(() =>
            {
                var instance = JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state);
                instance.Deleted = true;
                return instance;
            });

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var result = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveCurrentInstanceAsync_ReturnsSameObjectWithinSameRequest()
    {
        // Arrange
        var journeyName = "test-flow";
        var instanceId = CreateIdWithRandomExtensionOnly(journeyName, out var uniqueKey);
        var stateType = typeof(TestState);
        var state = new TestState();

        var stateProvider = new Mock<IUserInstanceStateProvider>();
        stateProvider
            .Setup(s => s.GetInstanceAsync(instanceId, stateType))
            .ReturnsAsync(() =>
                JourneyInstance.Create(
                    stateProvider.Object,
                    instanceId,
                    stateType,
                    state));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?ffiid={uniqueKey}");

        _options.Value.JourneyRegistry.RegisterJourney(
            new JourneyDescriptor(journeyName, stateType, [], appendUniqueKey: true));

        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.SetProperty(new ActionJourneyMetadata(journeyName));

        var actionContext = CreateActionContext(httpContext, actionDescriptor);

        var instanceProvider = new JourneyInstanceProvider(stateProvider.Object, _options);

        // Act
        var instance1 = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);
        var instance2 = await instanceProvider.ResolveCurrentInstanceAsync(actionContext);

        // Assert
        Assert.Same(instance1, instance2);
    }

    private static ActionContext CreateActionContext(
        HttpContext httpContext,
        ActionDescriptor actionDescriptor)
    {
        return new ActionContext(httpContext, httpContext.GetRouteData(), actionDescriptor);
    }

    private static JourneyInstanceId CreateIdWithRandomExtensionOnly(
        string journeyName,
        out string uniqueKey)
    {
        uniqueKey = Guid.NewGuid().ToString();

        return new JourneyInstanceId(
            journeyName,
            new Dictionary<string, StringValues>()
            {
                { Constants.UniqueKeyQueryParameterName, uniqueKey }
            });
    }

    private class TestState { }

    private class OtherTestState { }
}
