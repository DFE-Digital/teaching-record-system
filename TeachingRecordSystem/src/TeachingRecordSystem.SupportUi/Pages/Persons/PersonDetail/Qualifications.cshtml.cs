using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class QualificationsModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;

    public QualificationsModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _referenceDataCache = referenceDataCache;
    }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public ContactSearchSortByOption? SortBy { get; set; }

    public string? Name { get; set; }

    public MandatoryQualificationInfo[]? MandatoryQualifications { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var qualifications = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationsByContactIdQuery(PersonId));
        MandatoryQualifications = await MapMandatoryQualifications(qualifications!);

        return Page();
    }

    private Task<MandatoryQualificationInfo[]> MapMandatoryQualifications(dfeta_qualification[] qualifications)
    {
        var mqs = qualifications
            .Where(q => q.dfeta_Type == dfeta_qualification_dfeta_Type.MandatoryQualification)
            .OrderByDescending(q => q.dfeta_CompletionorAwardDate)
            .Select(async q =>
            {
                var mqEstablishment = q.dfeta_MQ_MQEstablishmentId is not null ? await _referenceDataCache.GetMqEstablishmentById(q.dfeta_MQ_MQEstablishmentId.Id) : null;
                var specialism = q.dfeta_MQ_SpecialismId is not null ? await _referenceDataCache.GetMqSpecialismById(q.dfeta_MQ_SpecialismId.Id) : null;

                return new MandatoryQualificationInfo
                {
                    QualificationId = q.Id,
                    Provider = mqEstablishment is not null ? mqEstablishment.dfeta_name : null,
                    Specialism = specialism is not null ? specialism.dfeta_name : null,
                    StartDate = q.dfeta_MQStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    EndDate = q.dfeta_MQ_Date.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    Result = q.dfeta_MQ_Status
                };
            });

        return Task.WhenAll(mqs);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personDetail = (ContactDetail?)context.HttpContext.Items["CurrentPersonDetail"];

        Name = personDetail!.Contact.ResolveFullName(includeMiddleName: false);

        await next();
    }

    public record MandatoryQualificationInfo
    {
        public required Guid QualificationId { get; init; }
        public required string? Provider { get; init; }
        public required string? Specialism { get; init; }
        public required DateOnly? StartDate { get; init; }
        public required DateOnly? EndDate { get; init; }
        public required dfeta_qualification_dfeta_MQ_Status? Result { get; init; }
    }
}
