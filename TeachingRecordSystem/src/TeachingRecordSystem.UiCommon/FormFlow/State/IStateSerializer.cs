namespace TeachingRecordSystem.UiCommon.FormFlow.State;

public interface IStateSerializer
{
    object Deserialize(Type type, string serialized);
    string Serialize(Type type, object state);
}
