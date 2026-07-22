using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public abstract class ResolveTeacherPensionsPotentialDuplicatePageModel(
    ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator journey,
    TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public string? BackLink { get; set; }

    public JourneyInstanceId InstanceId => journey.InstanceId;

    protected ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator Journey => journey;

    protected TrsDbContext DbContext => dbContext!;

    protected SupportTask GetSupportTask()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask;
    }

    /// This journey offers no middle name choice, so that source is always unset and the existing record's
    /// middle name is kept.
    protected PersonAttributeSources GetPersonAttributeSources()
    {
        var state = journey.State;

        if (!state.PersonAttributeSourcesSet)
        {
            throw new InvalidOperationException("Attribute sources not set.");
        }

        return new PersonAttributeSources()
        {
            FirstName = state.FirstNameSource,
            MiddleName = state.MiddleNameSource,
            LastName = state.LastNameSource,
            DateOfBirth = state.DateOfBirthSource,
            NationalInsuranceNumber = state.NationalInsuranceNumberSource,
            Gender = state.GenderSource
        };
    }

    protected async Task<TeacherPensionsPotentialDuplicateAttributes> GetPersonAttributesAsync(Guid personId)
    {
        var personAttributes = await DbContext.Persons
            .Where(p => p.PersonId == personId)
            .Select(p => new
            {
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.DateOfBirth,
                p.NationalInsuranceNumber,
                p.EmailAddress,
                p.Gender,
                p.Trn
            })
            .SingleAsync();

        return new TeacherPensionsPotentialDuplicateAttributes()
        {
            FirstName = personAttributes.FirstName,
            MiddleName = personAttributes.MiddleName,
            LastName = personAttributes.LastName,
            DateOfBirth = personAttributes.DateOfBirth,
            NationalInsuranceNumber = personAttributes.NationalInsuranceNumber,
            Gender = personAttributes.Gender,
            Trn = personAttributes.Trn
        };
    }

}

