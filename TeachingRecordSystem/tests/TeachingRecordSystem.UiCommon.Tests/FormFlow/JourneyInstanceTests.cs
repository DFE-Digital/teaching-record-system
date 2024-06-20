using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.UiCommon.FormFlow.Tests;

public class JourneyInstanceTests
{
    [Fact]
    public async Task DeleteAsync_CallsDeleteOnStateProvider()
    {
        // Arrange
        var instanceId = new JourneyInstanceId("instance", new Dictionary<string, StringValues>());

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var stateType = typeof(MyState);

        var instance = (JourneyInstance<MyState>)JourneyInstance.Create(
            stateProvider.Object,
            instanceId,
            stateType,
            new MyState(),
            properties: new Dictionary<object, object>());

        var newState = new MyState();

        // Act
        await instance.DeleteAsync();

        // Assert
        stateProvider.Verify(mock => mock.DeleteInstanceAsync(instanceId, stateType));
    }

    [Fact]
    public async Task CompleteAsync_CallsDeleteOnStateProvider()
    {
        // Arrange
        var instanceId = new JourneyInstanceId("instance", new Dictionary<string, StringValues>());

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var stateType = typeof(MyState);

        var instance = (JourneyInstance<MyState>)JourneyInstance.Create(
            stateProvider.Object,
            instanceId,
            stateType,
            new MyState(),
            properties: new Dictionary<object, object>());

        var newState = new MyState();

        // Act
        await instance.CompleteAsync();

        // Assert
        stateProvider.Verify(mock => mock.CompleteInstanceAsync(instanceId, stateType));
    }

    [Fact]
    public async Task UpdateStateAsync_DeletedInstance_ThrowsInvalidOperationException()
    {
        // Arrange
        var instanceId = new JourneyInstanceId("instance", new Dictionary<string, StringValues>());

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var stateType = typeof(MyState);

        var instance = (JourneyInstance<MyState>)JourneyInstance.Create(
            stateProvider.Object,
            instanceId,
            stateType,
            new MyState(),
            properties: new Dictionary<object, object>());

        var newState = new MyState();

        await instance.CompleteAsync();

        // Act
        var ex = await Record.ExceptionAsync(() => instance.UpdateStateAsync(newState));

        // Act & Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task UpdateStateAsync_CallsUpdateStateOnStateProvider()
    {
        // Arrange
        var instanceId = new JourneyInstanceId("instance", new Dictionary<string, StringValues>());

        var stateProvider = new Mock<IUserInstanceStateProvider>();

        var stateType = typeof(MyState);

        var instance = (JourneyInstance<MyState>)JourneyInstance.Create(
            stateProvider.Object,
            instanceId,
            stateType,
            new MyState(),
            properties: new Dictionary<object, object>());

        var newState = new MyState();

        // Act
        await instance.UpdateStateAsync(newState);

        // Assert
        stateProvider.Verify(mock => mock.UpdateInstanceStateAsync(instanceId, stateType, newState));
        Assert.Same(newState, instance.State);
    }

    public class MyState { }
}
