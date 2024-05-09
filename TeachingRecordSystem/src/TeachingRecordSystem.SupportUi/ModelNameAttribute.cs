namespace TeachingRecordSystem.SupportUi;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class ModelNameAttribute(string modelName) : Attribute
{
    public string ModelName { get; } = modelName;
}
