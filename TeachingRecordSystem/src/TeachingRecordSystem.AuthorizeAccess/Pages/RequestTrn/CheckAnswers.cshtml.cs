using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class CheckAnswersModel(
    AuthorizeAccessLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    IPersonMatchingService matchingService,
    IFileService fileService,
    IClock clock) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    public RequestTrnJourneyState? JourneyState { get; set; }

    public string? WorkEmail { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }

    public string? PreviousFirstName { get; set; }
    public string? PreviousMiddleName { get; set; }
    public string? PreviousLastName { get; set; }

    public string? Name => StringHelper.JoinNonEmpty(' ', new string?[] { FirstName, MiddleName, LastName });
    public string? PreviousName => StringHelper.JoinNonEmpty(' ', new string?[] { PreviousFirstName, PreviousMiddleName, PreviousLastName });

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

        var trnRequestMetadata = new TrnRequestMetadata
        {
            OneLoginUserSubject = null,
            CreatedOn = clock.UtcNow,
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
            Name = GetNonEmptyValues(state.FirstName, state.MiddleName, state.LastName),
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

        // look for potential matches
        var matchResult = await matchingService.MatchFromTrnRequestAsync(trnRequestMetadata);
        trnRequestMetadata.PotentialDuplicate = matchResult.Outcome is not TrnRequestMatchResultOutcome.NoMatches;

        trnRequestMetadata.Matches = new TrnRequestMatches()
        {
            MatchedPersons = matchResult.Outcome switch
            {
                TrnRequestMatchResultOutcome.PotentialMatches =>
                    matchResult.PotentialMatchesPersonIds
                        .Select(id => new TrnRequestMatchedPerson() { PersonId = id })
                        .ToList(),
                TrnRequestMatchResultOutcome.DefiniteMatch => [new TrnRequestMatchedPerson() { PersonId = matchResult.PersonId }],
                _ => []
            }
        };

        dbContext.TrnRequestMetadata.Add(trnRequestMetadata);

        var supportTask = SupportTask.Create(
            supportTaskType: SupportTaskType.NpqTrnRequest,
            data: new NpqTrnRequestData(),
            personId: null,
            oneLoginUserSubject: null,
            trnRequestApplicationUserId: ApplicationUser.NpqApplicationUserGuid,
            trnRequestId: requestId,
            createdBy: ApplicationUser.NpqApplicationUserGuid,
            now: clock.UtcNow,
            out var createdEvent
            );
        dbContext.SupportTasks.Add(supportTask);

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(createdEvent);

        await JourneyInstance!.UpdateStateAsync(state => state.HasPendingTrnRequest = true);

        return Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));

        static string[] GetNonEmptyValues(params string?[] values)
        {
            var result = new List<string>(values.Length);

            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }

            return result.ToArray();
        }
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.HaveRegisteredForAnNpq is null && state.NpqApplicationId is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnNpqApplication(JourneyInstance.InstanceId));
        }
        else if (state.WorkEmail is null && state.WorkingInSchoolOrEducationalSetting == true)
        {
            context.Result = Redirect(linkGenerator.RequestTrnWorkEmail(JourneyInstance.InstanceId));
        }
        else if (state.PersonalEmail is null)
        {
            // personal email is required for either WorkingInSchoolOrEducationalSetting being true or false
            context.Result = Redirect(linkGenerator.RequestTrnPersonalEmail(JourneyInstance.InstanceId));
        }
        else if (state.FirstName is null || state.LastName is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnName(JourneyInstance.InstanceId));
        }
        else if (state.HasPreviousName is null || (state.HasPreviousName == true && (state.PreviousFirstName is null || state.PreviousLastName is null)))
        {
            context.Result = Redirect(linkGenerator.RequestTrnPreviousName(JourneyInstance.InstanceId));
        }
        else if (state.DateOfBirth is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnDateOfBirth(JourneyInstance.InstanceId));
        }
        else if (state.EvidenceFileId is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnIdentity(JourneyInstance!.InstanceId));
        }
        else if (state.HasNationalInsuranceNumber is null || (state.HasNationalInsuranceNumber == true && state.NationalInsuranceNumber is null))
        {
            context.Result = Redirect(linkGenerator.RequestTrnNationalInsuranceNumber(JourneyInstance.InstanceId));
        }
        else if (state.HasNationalInsuranceNumber == false &&
            (state.AddressLine1 is null || state.TownOrCity is null || state.PostalCode is null || state.Country is null))
        {
            context.Result = Redirect(linkGenerator.RequestTrnAddress(JourneyInstance.InstanceId));
        }
    }
}
