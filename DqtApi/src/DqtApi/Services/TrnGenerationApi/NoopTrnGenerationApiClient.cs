namespace DqtApi.Services.TrnGenerationApi
{
    using System.Threading.Tasks;

    public class NoopTrnGenerationApiClient : ITrnGenerationApiClient
    {
        public Task<string> GenerateTrnAsync()
        {
            string trn = null;
            return Task.FromResult(trn);
        }
    }
}
