using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class CheckAnswersModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;
    private readonly TrsLinkGenerator _linkGenerator;

    public CheckAnswersModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache,
        TrsLinkGenerator linkGenerator)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _referenceDataCache = referenceDataCache;
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public dfeta_mqestablishment? MqEstablishment { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public DateOnly? StartDate { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var mqSpecialism = await _referenceDataCache.GetMqSpecialismByValue(Specialism!.Value.GetDqtValue());

        await _crmQueryDispatcher.ExecuteQuery(
            new CreateMandatoryQualificationQuery()
            {
                ContactId = PersonId,
                MqEstablishmentId = MqEstablishment!.Id,
                SpecialismId = mqSpecialism.Id,
                StartDate = StartDate!.Value,
                Status = Status!.Value.GetDqtStatus(),
                EndDate = Status == MandatoryQualificationStatus.Passed
                    ? EndDate!.Value
                    : null
            });

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification added");

        return Redirect(_linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonDetail(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personDetail = (ContactDetail?)context.HttpContext.Items["CurrentPersonDetail"];

        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(_linkGenerator.MqAddProvider(PersonId, JourneyInstance.InstanceId));
        }

        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        MqEstablishment = await _referenceDataCache.GetMqEstablishmentByValue(JourneyInstance!.State.MqEstablishmentValue!);
        Specialism = JourneyInstance.State.Specialism;
        StartDate = JourneyInstance.State.StartDate;
        Status = JourneyInstance.State.Status;
        EndDate = JourneyInstance.State.EndDate;

        await next();
    }
}
