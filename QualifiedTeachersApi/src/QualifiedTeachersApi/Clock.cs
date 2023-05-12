namespace QualifiedTeachersApi;

public sealed class Clock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
