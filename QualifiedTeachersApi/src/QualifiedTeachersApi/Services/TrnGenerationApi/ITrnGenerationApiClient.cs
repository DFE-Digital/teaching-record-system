using System.Threading.Tasks;

namespace QualifiedTeachersApi.Services.TrnGenerationApi
{
    public interface ITrnGenerationApiClient
    {
        Task<string> GenerateTrn();
    }
}
