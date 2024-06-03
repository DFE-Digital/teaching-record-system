using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.NameSynonyms;

namespace TeachingRecordSystem.Core.Jobs;

public class PopulateNameSynonymsJob(TrsDbContext dbContext, INameSynonymProvider nameSynonymProvider)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        var namesLookup = await nameSynonymProvider.GetAllNameSynonyms();

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
