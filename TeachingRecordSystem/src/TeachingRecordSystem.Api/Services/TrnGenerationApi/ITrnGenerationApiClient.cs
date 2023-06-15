namespace TeachingRecordSystem.Api.Services.TrnGenerationApi;

public interface ITrnGenerationApiClient
{
    Task<string> GenerateTrn();
}
