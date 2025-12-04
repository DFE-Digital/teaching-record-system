
namespace TeachingRecordSystem.Core.Services.Persons;

[Serializable]
public class TrsPersonNotFoundException : Exception
{
    public TrsPersonNotFoundException()
    {
    }

    public TrsPersonNotFoundException(string? message) : base(message)
    {
    }

    public TrsPersonNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
