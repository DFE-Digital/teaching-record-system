using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

public class PopulateNameSynonymsJob(TrsDbContext dbContext, HttpClient httpClient)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        using var stream = await httpClient.GetStreamAsync("https://raw.githubusercontent.com/carltonnorthern/nicknames/master/names.csv");
        using var reader = new StreamReader(stream);

        var namesLookup = new Dictionary<string, HashSet<string>>();
        string? line = null;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length == 0 || line[0] == '#')
            {
                continue; // ignore empty lines and comments
            }

            var names = line.Split(',');
            foreach (var name in names)
            {
                HashSet<string>? synonyms;
                if (!namesLookup.TryGetValue(name, out synonyms))
                {
                    synonyms = new HashSet<string>();
                    namesLookup[name] = synonyms;
                }

                foreach (var altName in names)
                {
                    // Don't add anything as a synonym of itself
                    if (altName != name)
                    {
                        synonyms.Add(altName);
                    }
                }
            }
        }

        foreach (var (name, synonyms) in namesLookup)
        {
            var nameSynonyms = await dbContext.NameSynonyms
                .Where(ns => ns.Name == name)
                .SingleOrDefaultAsync(cancellationToken);

            if (nameSynonyms == null)
            {
                nameSynonyms = new NameSynonyms
                {
                    Name = name,
                    Synonyms = synonyms.ToArray(),
                };

                dbContext.NameSynonyms.Add(nameSynonyms);
            }
            else
            {
                nameSynonyms.Synonyms = synonyms.ToArray();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
