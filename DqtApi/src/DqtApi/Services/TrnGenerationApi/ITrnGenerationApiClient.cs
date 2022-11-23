namespace DqtApi.Services.TrnGenerationApi
{
    using System.Threading.Tasks;

    public interface ITrnGenerationApiClient
    {
        Task<string> GenerateTrnAsync();
    }
}
