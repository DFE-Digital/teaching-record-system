using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class CheckAnswersModel(AuthorizeAccessLinkGenerator linkGenerator, TrsDbContext dbContext, IPersonMatchingService matchingService, IFileService fileService) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    public RequestTrnJourneyState? JourneyState { get; set; }

    public string? WorkEmail { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string? Name => StringHelper.JoinNonEmpty(' ', new string?[] { FirstName, MiddleName, LastName });
    public string? PreviousName { get; set; }

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
        PreviousName = JourneyInstance!.State.PreviousName;
        DateOfBirth = JourneyInstance!.State.DateOfBirth;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance!.State.EvidenceFileId!.Value, _fileUrlExpiresAfter) :
            null;
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
        var requestId = Guid.NewGuid().ToString(); // CML TODO - does this have to be constructed such that duplicate requests are recognised?

        var trnRequestMetadata = new TrnRequestMetadata
        {
            OneLoginUserSubject = null,
            CreatedOn = DateTime.UtcNow,
            RequestId = requestId, // CML TODO - how to set this?
            IdentityVerified = false,
            ApplicationUserId = ApplicationUser.NPQApplicationUserGuid,
            FirstName = state.FirstName,
            MiddleName = state.MiddleName,
            LastName = state.LastName,
            Name = new[] { state.FirstName!, state.MiddleName ?? string.Empty, state.LastName! }, // CML TODO - use individual name vars or this array?
            EmailAddress = state.PersonalEmail,
            //PreviousName = state.PreviousName,
            DateOfBirth = state.DateOfBirth!.Value,
            NationalInsuranceNumber = Core.NationalInsuranceNumber.Normalize(state.NationalInsuranceNumber),
            NpqApplicationId = state.NpqApplicationId,
            NpqName = state.NpqName,
            NpqTrainingProvider = state.NpqTrainingProvider
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

        dbContext.SupportTasks.Add(new SupportTask
        {
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            TrnRequestId = requestId,
            Status = SupportTaskStatus.Open,
            SupportTaskType = SupportTaskType.NpqTrnRequest,
            Data = new NpqTrnRequestData(), // CML TODO - is this correct?
            OneLoginUserSubject = null,
            PersonId = null,
            SupportTaskReference = SupportTask.GenerateSupportTaskReference(),
            TrnRequestApplicationUserId = ApplicationUser.NPQApplicationUserGuid
        });
        dbContext.SaveChanges(); // CML TODO - what if it fails?
        // CML TODO - ensure the file upload happens successfully along with the DB update?
        // CML TODO - clarify what's to happen with the fields that are submitted but not used by the console app

        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        if (!fileExtensionContentTypeProvider.TryGetContentType(JourneyInstance!.State.EvidenceFileName!, out var evidenceFileMimeType))
        {
            evidenceFileMimeType = "application/octet-stream";
        }

        using var stream = await fileService.OpenReadStreamAsync(JourneyInstance!.State.EvidenceFileId!.Value);


        await JourneyInstance!.UpdateStateAsync(state => state.HasPendingTrnRequest = true); // CML TODO understand this - To stop duplicates?, but how is their reference preserved?


        return Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
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
        else if (state.HasPreviousName is null || (state.HasPreviousName == true && state.PreviousName is null))
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
