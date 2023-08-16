namespace TeachingRecordSystem.Core.Services.TrnGenerationApi;

public interface ITrnGenerationApiClient
{
    Task<string> GenerateTrn();
}
