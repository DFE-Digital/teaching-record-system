
namespace TeachingRecordSystem.Core.Services.NameSynonyms;

public interface INameSynonymProvider
{
    Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetAllNameSynonyms();
}
