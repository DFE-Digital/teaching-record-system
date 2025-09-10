namespace TeachingRecordSystem.Core.Models.SupportTaskData;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SupportTaskDataAttribute(string supportTaskTypeId) : Attribute
{
    public Guid SupportTaskTypeId { get; } = new(supportTaskTypeId);
}
