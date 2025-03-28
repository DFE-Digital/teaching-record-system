using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class QualificationsModel(TrsDbContext dbContext, ReferenceDataCache referenceDataCache) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public ContactSearchSortByOption? SortBy { get; set; }

    public MandatoryQualification[]? MandatoryQualifications { get; set; }
    public ProfessionalStatus[]? ProfessionalStatuses { get; set; }
    public Dictionary<Guid, string>? TrainingSubjects { get; set; }

    public async Task OnGetAsync()
    {
        ProfessionalStatuses = await dbContext.ProfessionalStatuses
            .Include(q => q.TrainingProvider)
            .Include(q => q.TrainingCountry)
            .Include(q => q.Route)
            .Include(q => q.DegreeType)
            .Where(x => x.PersonId == PersonId)
            .OrderBy(x => x.CreatedOn)
            .ToArrayAsync();
        var uniqueSubjectIds = ProfessionalStatuses
            .SelectMany(x => x.TrainingSubjectIds ?? Array.Empty<Guid>())
            .Distinct()
            .ToArray();
        var trainingSubjectsLookup = (await referenceDataCache.GetTrainingSubjectsAsync())
            .ToDictionary(subject => subject.TrainingSubjectId, subject => subject.Name);
        TrainingSubjects = uniqueSubjectIds.ToDictionary(
            id => id,
            id => trainingSubjectsLookup[id]
        );

        MandatoryQualifications = await dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .Where(q => q.PersonId == PersonId)
            .ToArrayAsync();
    }
}
