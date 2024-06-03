namespace TeachingRecordSystem.Core.Services.NameSynonyms;

public class NameSynonymProvider(HttpClient httpClient) : INameSynonymProvider
{
    private Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>? _synonymsTask;

    public Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetAllNameSynonyms() =>
        LazyInitializer.EnsureInitialized(ref _synonymsTask, () => DownloadAllNameSynonyms());

    private async Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> DownloadAllNameSynonyms()
    {
        using var stream = await httpClient.GetStreamAsync("https://raw.githubusercontent.com/carltonnorthern/nicknames/master/names.csv");
        using var reader = new StreamReader(stream);

        var namesLookup = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        string? line = null;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            var names = line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach (var name in names)
            {
                if (!namesLookup.TryGetValue(name, out var synonyms))
                {
                    synonyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    namesLookup[name] = synonyms;
                }

                foreach (var altName in names)
                {
                    if (altName != name)
                    {
                        synonyms.Add(altName);
                    }
                }
            }
        }

        return namesLookup.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsReadOnly());
    }
}
