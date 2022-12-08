using System.Threading.Tasks;

namespace DqtApi.Services.TrnGenerationApi
{
    public interface ITrnGenerationApiClient
    {
        Task<string> GenerateTrn();
    }
}
