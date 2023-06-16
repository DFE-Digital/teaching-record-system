namespace TeachingRecordSystem.Api.Events;

/// <summary>
/// A dummy event to be able to test the background publishing service.
/// </summary>
/// <remarks>
/// This can be removed and replaced in the tests by a "real" event once we've create some.
/// </remarks>
public record DummyEvent : EventBase
{
    public required string DummyProperty { get; init; }
}
