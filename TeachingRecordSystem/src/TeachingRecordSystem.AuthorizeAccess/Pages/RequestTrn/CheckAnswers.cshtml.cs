using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class CheckAnswersModel(AuthorizeAccessLinkGenerator linkGenerator, ICrmQueryDispatcher crmQueryDispatcher, IFileService fileService) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    public RequestTrnJourneyState? JourneyState { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

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

    public async Task OnGet()
    {
        Email = JourneyInstance!.State.Email;
        Name = JourneyInstance!.State.Name;
        PreviousName = JourneyInstance!.State.PreviousName;
        DateOfBirth = JourneyInstance!.State.DateOfBirth;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrl(JourneyInstance!.State.EvidenceFileId!.Value, _fileUrlExpiresAfter) :
            null;
        HasNationalInsuranceNumber = JourneyInstance!.State.HasNationalInsuranceNumber;
        NationalInsuranceNumber = JourneyInstance!.State.NationalInsuranceNumber;
        AddressLine1 = JourneyInstance!.State.AddressLine1;
        AddressLine2 = JourneyInstance!.State.AddressLine2;
        TownOrCity = JourneyInstance!.State.TownOrCity;
        PostalCode = JourneyInstance!.State.PostalCode;
        Country = JourneyInstance!.State.Country;
    }

    public async Task<IActionResult> OnPost()
    {
        var state = JourneyInstance!.State;

        var description = $"""
            Email: {state.Email}
            Name: {state.Name}
            Previous name: {state.PreviousName}
            Date of birth: {state.DateOfBirth:dd/MM/yyyy}                
            National Insurance number: {NationalInsuranceNumberHelper.Normalize(state.NationalInsuranceNumber)}
            """;
        if (state.HasNationalInsuranceNumber == false)
        {
            description += $"""

                Address line 1: {state.AddressLine1}
                Address line 2: {state.AddressLine2}
                Town or city: {state.TownOrCity}
                Postal code: {state.PostalCode}
                Country: {state.Country}
                """;
        }

        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        if (!fileExtensionContentTypeProvider.TryGetContentType(JourneyInstance!.State.EvidenceFileName!, out var evidenceFileMimeType))
        {
            evidenceFileMimeType = "application/octet-stream";
        }

        using var stream = await fileService.OpenReadStream(JourneyInstance!.State.EvidenceFileId!.Value);

        await crmQueryDispatcher.ExecuteQuery(
            new CreateTrnRequestTaskQuery()
            {
                Description = description,
                EvidenceFileName = JourneyInstance!.State.EvidenceFileName!,
                EvidenceFileContent = stream,
                EvidenceFileMimeType = evidenceFileMimeType
            });

        await JourneyInstance!.UpdateStateAsync(state => state.HasPendingTrnRequest = true);

        return Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.Email is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnEmail(JourneyInstance.InstanceId));
        }
        else if (state.Name is null)
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
