using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class CheckAnswersModel(
    RequestTrnLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    TrnRequestService trnRequestService,
    SupportTaskService supportTaskService,
    IEventPublisher eventPublisher,
    IFileService fileService,
    IClock clock) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    public string? WorkEmail { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }

    public string? PreviousFirstName { get; set; }
    public string? PreviousMiddleName { get; set; }
    public string? PreviousLastName { get; set; }

    public string Name => StringExtensions.JoinNonEmpty(' ', FirstName, MiddleName, LastName);
    public string PreviousName => StringExtensions.JoinNonEmpty(' ', PreviousFirstName, PreviousMiddleName, PreviousLastName);

    public DateOnly? DateOfBirth { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public bool? HasNationalInsuranceNumber { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? TownOrCity { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? NpqApplicationId { get; set; }

    public string? PersonalEmail { get; set; }

    public string? NpqName { get; set; }

    public string? NpqProvider { get; set; }

    public async Task OnGetAsync()
    {
        WorkEmail = JourneyInstance!.State.WorkEmail;
        FirstName = JourneyInstance!.State.FirstName;
        MiddleName = JourneyInstance!.State.MiddleName;
        LastName = JourneyInstance!.State.LastName;
        PreviousFirstName = JourneyInstance!.State.PreviousFirstName;
        PreviousMiddleName = JourneyInstance!.State.PreviousMiddleName;
        PreviousLastName = JourneyInstance!.State.PreviousLastName;
        DateOfBirth = JourneyInstance!.State.DateOfBirth;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
        UploadedEvidenceFileUrl = await fileService.GetFileUrlAsync(JourneyInstance!.State.EvidenceFileId!.Value, _fileUrlExpiresAfter);
        HasNationalInsuranceNumber = JourneyInstance!.State.HasNationalInsuranceNumber;
        NationalInsuranceNumber = JourneyInstance!.State.NationalInsuranceNumber;
        AddressLine1 = JourneyInstance!.State.AddressLine1;
        AddressLine2 = JourneyInstance!.State.AddressLine2;
        TownOrCity = JourneyInstance!.State.TownOrCity;
        PostalCode = JourneyInstance!.State.PostalCode;
        Country = JourneyInstance!.State.Country;
        NpqApplicationId = JourneyInstance!.State.NpqApplicationId;
        PersonalEmail = JourneyInstance!.State.PersonalEmail;
        NpqName = JourneyInstance!.State.NpqName;
        NpqProvider = JourneyInstance!.State.NpqTrainingProvider;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var state = JourneyInstance!.State;
        var requestId = Guid.NewGuid().ToString();

        var processContext = new ProcessContext(ProcessType.NpqTrnRequestTaskCreating, clock.UtcNow, ApplicationUser.NpqApplicationUserGuid);

        var trnRequest = new TrnRequestMetadata
        {
            OneLoginUserSubject = null,
            CreatedOn = processContext.Now,
            RequestId = requestId,
            IdentityVerified = false,
            ApplicationUserId = ApplicationUser.NpqApplicationUserGuid,
            FirstName = state.FirstName,
            MiddleName = state.MiddleName,
            LastName = state.LastName,
            PreviousFirstName = state.PreviousFirstName,
            PreviousMiddleName = state.PreviousMiddleName,
            PreviousLastName = state.PreviousLastName,
            WorkEmailAddress = state.WorkEmail,
            Name = new[] { state.FirstName, state.MiddleName, state.LastName }.GetNonEmptyValues(),
            EmailAddress = state.PersonalEmail,
            DateOfBirth = state.DateOfBirth!.Value,
            NationalInsuranceNumber = Core.NationalInsuranceNumber.Normalize(state.NationalInsuranceNumber),
            NpqApplicationId = state.NpqApplicationId,
            NpqName = state.NpqName,
            NpqTrainingProvider = state.NpqTrainingProvider,
            AddressLine1 = state.AddressLine1,
            AddressLine2 = state.AddressLine2,
            Postcode = state.PostalCode,
            City = state.TownOrCity,
            Country = state.Country,
            NpqEvidenceFileId = state.EvidenceFileId,
            NpqEvidenceFileName = state.EvidenceFileName,
            NpqWorkingInEducationalSetting = state.WorkingInSchoolOrEducationalSetting
        };

        // Required to assign the PotentialDuplicate property on the TRN request
        await trnRequestService.MatchPersonsAsync(trnRequest);

        dbContext.TrnRequestMetadata.Add(trnRequest);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new TrnRequestCreatedEvent
            {
                EventId = Guid.NewGuid(),
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequest)
            },
            processContext);

        await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.NpqTrnRequest,
                Data = new NpqTrnRequestData(),
                PersonId = null,
                OneLoginUserSubject = null,
                TrnRequest = (trnRequest.ApplicationUserId, trnRequest.RequestId)
            },
            processContext);

        await JourneyInstance!.UpdateStateAsync(state => state.HasPendingTrnRequest = true);

        return Redirect(linkGenerator.Submitted(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.Submitted(JourneyInstance!.InstanceId));
        }
        else if (state.HaveRegisteredForAnNpq is null && state.NpqApplicationId is null)
        {
            context.Result = Redirect(linkGenerator.NpqApplication(JourneyInstance.InstanceId));
        }
        else if (state.WorkEmail is null && state.WorkingInSchoolOrEducationalSetting == true)
        {
            context.Result = Redirect(linkGenerator.WorkEmail(JourneyInstance.InstanceId));
        }
        else if (state.PersonalEmail is null)
        {
            // personal email is required for either WorkingInSchoolOrEducationalSetting being true or false
            context.Result = Redirect(linkGenerator.PersonalEmail(JourneyInstance.InstanceId));
        }
        else if (state.FirstName is null || state.LastName is null)
        {
            context.Result = Redirect(linkGenerator.Name(JourneyInstance.InstanceId));
        }
        else if (state.HasPreviousName is null || (state.HasPreviousName == true && (state.PreviousFirstName is null || state.PreviousLastName is null)))
        {
            context.Result = Redirect(linkGenerator.PreviousName(JourneyInstance.InstanceId));
        }
        else if (state.DateOfBirth is null)
        {
            context.Result = Redirect(linkGenerator.DateOfBirth(JourneyInstance.InstanceId));
        }
        else if (state.EvidenceFileId is null)
        {
            context.Result = Redirect(linkGenerator.Identity(JourneyInstance!.InstanceId));
        }
        else if (state.HasNationalInsuranceNumber is null || (state.HasNationalInsuranceNumber == true && state.NationalInsuranceNumber is null))
        {
            context.Result = Redirect(linkGenerator.NationalInsuranceNumber(JourneyInstance.InstanceId));
        }
        else if (state.HasNationalInsuranceNumber == false &&
            (state.AddressLine1 is null || state.TownOrCity is null || state.PostalCode is null || state.Country is null))
        {
            context.Result = Redirect(linkGenerator.Address(JourneyInstance.InstanceId));
        }
    }
}
