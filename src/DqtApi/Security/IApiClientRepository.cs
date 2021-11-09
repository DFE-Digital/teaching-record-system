namespace DqtApi.Security
{
    public interface IApiClientRepository
    {
        ApiClient GetClientByKey(string apiKey);
    }
}
