namespace QualifiedTeachersApi.Security
{
    public interface ICurrentClientProvider
    {
        string GetCurrentClientId();
    }
}
