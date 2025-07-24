using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.NoMatches;

public class CheckAnswersModel(
    TrsDbContext dbContext,
    ITrnGenerator trnGenerator,
    TrsLinkGenerator linkGenerator,
    IClock clock) : PageModel
{
    public string? SourceApplicationUserName { get; set; }
    public Guid? SourceApplicationUserId { get; set; }

    public string? SupportTaskReference { get; set; }
    public Guid? PersonId { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public Gender? Gender { get; set; }

    public string? Trn { get; set; }

    public bool PersonNameChange { get; set; }
    public bool PersonDetailsChange { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);

        var trn = await trnGenerator.GenerateTrnAsync();
        var person = Person.Create(
            trn,
            requestData.FirstName ?? string.Empty,
            requestData.MiddleName ?? string.Empty,
            requestData.LastName ?? string.Empty,
            requestData.DateOfBirth,
            requestData.EmailAddress is not null ? Core.EmailAddress.Parse(requestData.EmailAddress) : null,
            requestData.NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(requestData.NationalInsuranceNumber) : null,
            clock.UtcNow);
        requestData.SetResolvedPerson(person.PersonId);
        dbContext.Add(person);

        var personAttributes = new NpqTrnRequestDataPersonAttributes()
        {
            DateOfBirth = requestData.DateOfBirth,
            EmailAddress = requestData.EmailAddress,
            FirstName = requestData.FirstName ?? string.Empty,
            MiddleName = requestData.MiddleName ?? string.Empty,
            LastName = requestData.LastName ?? string.Empty,
            NationalInsuranceNumber = requestData.NationalInsuranceNumber
        };

        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = clock.UtcNow;
        supportTask.UpdateData<NpqTrnRequestData>(data => data with
        {
            ResolvedAttributes = personAttributes,
            SelectedPersonAttributes = null
        });

        var @event = new NpqTrnRequestSupportTaskCreatedPersonEvent()
        {
            PersonId = requestData.ResolvedPersonId!.Value,
            PersonDetails = EventModels.PersonDetails.FromModel(person),
            SupportTask = EventModels.SupportTask.FromModel(supportTask),
            OldSupportTask = oldSupportTaskEventModel,
            RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
            Comments = null,
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId()
        };

        await dbContext.AddEventAndBroadcastAsync(@event);

        await dbContext.SaveChangesAsync();

        // This is a little ugly but pushing this into a partial and executing it here is tricky
        var flashMessageHtml =
            $@"
            <a href=""{linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value)}"" class=""govuk-link"">View record</a>
            ";

        var message = "Record created for";
        TempData.SetFlashSuccess(
            $"{message} {FirstName} {MiddleName} {LastName}",
            messageHtml: flashMessageHtml);

        return Redirect(linkGenerator.SupportTasks());
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.SupportTasks());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;

        PersonId = Guid.NewGuid();

        FirstName = requestData.FirstName;
        MiddleName = requestData.MiddleName;
        LastName = requestData.LastName;
        DateOfBirth = requestData.DateOfBirth;
        EmailAddress = requestData.EmailAddress;
        NationalInsuranceNumber = requestData.NationalInsuranceNumber;
        Trn = null;
        SupportTaskReference = supportTask.SupportTaskReference;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;
        SourceApplicationUserId = requestData.ApplicationUser!.UserId;

        return next();
    }
}
