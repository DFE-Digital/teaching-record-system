using System.Threading.Tasks;

namespace DqtApi.Services.TrnGenerationApi;

public class NoopTrnGenerationApiClient : ITrnGenerationApiClient
{
    public Task<string> GenerateTrn()
    {
        string trn = null;
        return Task.FromResult(trn);
    }
}
