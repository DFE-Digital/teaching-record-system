namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public interface ITrnGenerator
{
    Task<string> GenerateTrnAsync();
}
