using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

public abstract class ResolveTrnRequestPageModel(
    ResolveTrnRequestJourneyCoordinator journey,
    TrsDbContext dbContext) : PageModel
{
    public string? BackLink { get; set; }

    protected ResolveTrnRequestJourneyCoordinator Journey => journey;

    protected TrsDbContext DbContext => dbContext;

    protected TrnRequestMetadata GetRequestData()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask.TrnRequestMetadata!;
    }

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
            EmailAddress = state.EmailAddressSource,
            NationalInsuranceNumber = state.NationalInsuranceNumberSource,
            Gender = state.GenderSource
        };
    }

    protected async Task<TrnRequestDataPersonAttributes> GetPersonAttributesAsync(Guid personId)
    {
        var personAttributes = await dbContext.Persons
            .Where(p => p.PersonId == personId)
            .Select(p => new
            {
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.DateOfBirth,
                p.NationalInsuranceNumber,
                p.EmailAddress,
                p.Gender
            })
            .SingleAsync();

        return new TrnRequestDataPersonAttributes()
        {
            FirstName = personAttributes.FirstName,
            MiddleName = personAttributes.MiddleName,
            LastName = personAttributes.LastName,
            DateOfBirth = personAttributes.DateOfBirth,
            EmailAddress = personAttributes.EmailAddress,
            NationalInsuranceNumber = personAttributes.NationalInsuranceNumber,
            Gender = personAttributes.Gender
        };
    }

}

