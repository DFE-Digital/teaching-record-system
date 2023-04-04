namespace QualifiedTeachersApi.V3;

public interface IConditionallySerializedProperties
{
    bool ShouldSerializeProperty(string propertyName);
}
