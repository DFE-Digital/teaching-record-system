using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
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

        var (person, _) = Person.Create(
            trn,
            requestData.FirstName ?? string.Empty,
            requestData.MiddleName ?? string.Empty,
            requestData.LastName ?? string.Empty,
            requestData.DateOfBirth,
            requestData.EmailAddress is not null ? Core.EmailAddress.Parse(requestData.EmailAddress) : null,
            requestData.NationalInsuranceNumber is not null ? Core.NationalInsuranceNumber.Parse(requestData.NationalInsuranceNumber) : null,
            requestData.Gender,
            clock.UtcNow,
            sourceTrnRequest: (requestData.ApplicationUserId, requestData.RequestId));

        dbContext.Add(person);

        requestData.SetResolvedPerson(person.PersonId);

        var personAttributes = new NpqTrnRequestDataPersonAttributes()
        {
            DateOfBirth = requestData.DateOfBirth,
            EmailAddress = requestData.EmailAddress,
            FirstName = requestData.FirstName ?? string.Empty,
            MiddleName = requestData.MiddleName ?? string.Empty,
            LastName = requestData.LastName ?? string.Empty,
            NationalInsuranceNumber = requestData.NationalInsuranceNumber,
            Gender = requestData.Gender
        };

        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = clock.UtcNow;
        supportTask.UpdateData<NpqTrnRequestData>(data => data with
        {
            SupportRequestOutcome = SupportRequestOutcome.Approved,
            ResolvedAttributes = personAttributes,
            SelectedPersonAttributes = null
        });

        var @event = new NpqTrnRequestSupportTaskResolvedEvent()
        {
            PersonId = requestData.ResolvedPersonId!.Value,
            RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
            ChangeReason = NpqTrnRequestResolvedReason.RecordCreated,
            Changes = 0,
            PersonAttributes = EventModels.PersonAttributes.FromModel(person),
            OldPersonAttributes = null,
            SupportTask = EventModels.SupportTask.FromModel(supportTask),
            OldSupportTask = oldSupportTaskEventModel,
            Comments = null,
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId()
        };

        await dbContext.AddEventAndBroadcastAsync(@event);
        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"{SourceApplicationUserName} request completed",
            buildMessageHtml: b =>
            {
                var link = new TagBuilder("a");
                link.AddCssClass("govuk-link");
                link.MergeAttribute("href", linkGenerator.PersonDetail(requestData.ResolvedPersonId!.Value));
                link.InnerHtml.Append($"Record created for {FirstName} {MiddleName} {LastName}");
                b.AppendHtml(link);
            });

        return Redirect(linkGenerator.NpqTrnRequests());
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.NpqTrnRequests());
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
        SupportTaskReference = supportTask.SupportTaskReference;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;
        SourceApplicationUserId = requestData.ApplicationUser!.UserId;

        return next();
    }
}
