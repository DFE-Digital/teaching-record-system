namespace TeachingRecordSystem.WebCommon.FormFlow.State;

public interface IStateSerializer
{
    object Deserialize(Type type, string serialized);
    string Serialize(Type type, object state);
}
