using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[Authorize(Policy = AuthorizationPolicies.NonPersonOrAlertDataView)]
[AllowDeactivatedPerson]
public class QualificationsModel(TrsDbContext dbContext, ReferenceDataCache referenceDataCache) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public PersonSearchSortByOption? SortBy { get; set; }

    public MandatoryQualification[]? MandatoryQualifications { get; set; }
    public RouteToProfessionalStatus[]? ProfessionalStatuses { get; set; }
    public Dictionary<Guid, string>? TrainingSubjects { get; set; }

    public async Task OnGetAsync()
    {
        ProfessionalStatuses = await dbContext.RouteToProfessionalStatuses
            .Where(x => x.PersonId == PersonId)
            .OrderBy(x => x.CreatedOn)
            .ToArrayAsync();
        var uniqueSubjectIds = ProfessionalStatuses
            .SelectMany(x => x.TrainingSubjectIds)
            .Distinct()
            .ToArray();
        var trainingSubjectsLookup = (await referenceDataCache.GetTrainingSubjectsAsync())
            .ToDictionary(subject => subject.TrainingSubjectId, subject => $"{subject.Reference} - {subject.Name}");
        TrainingSubjects = uniqueSubjectIds.ToDictionary(
            id => id,
            id => trainingSubjectsLookup[id]
        );

        MandatoryQualifications = await dbContext.MandatoryQualifications
            .Where(q => q.PersonId == PersonId)
            .ToArrayAsync();
    }
}
