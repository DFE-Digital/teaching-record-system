using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class QualificationsModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache) : PageModel
{
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
        var qualifications = await crmQueryDispatcher.ExecuteQuery(new GetQualificationsByContactIdQuery(PersonId));
        MandatoryQualifications = await MapMandatoryQualifications(qualifications!);

        return Page();
    }

    private Task<MandatoryQualificationInfo[]> MapMandatoryQualifications(dfeta_qualification[] qualifications)
    {
        var mqs = qualifications
            .Where(q => q.dfeta_Type == dfeta_qualification_dfeta_Type.MandatoryQualification)
            .OrderByDescending(q => q.dfeta_CompletionorAwardDate)
            .ThenByDescending(q => q.CreatedOn)
            .Select(async q =>
            {
                var mqEstablishment = q.dfeta_MQ_MQEstablishmentId is not null ? await referenceDataCache.GetMqEstablishmentById(q.dfeta_MQ_MQEstablishmentId.Id) : null;
                var specialism = q.dfeta_MQ_SpecialismId is not null ? await referenceDataCache.GetMqSpecialismById(q.dfeta_MQ_SpecialismId.Id) : null;
                var status = q.dfeta_MQ_Status ?? (q.dfeta_MQ_Date.HasValue ? dfeta_qualification_dfeta_MQ_Status.Passed : null);

                return new MandatoryQualificationInfo
                {
                    QualificationId = q.Id,
                    Provider = mqEstablishment is not null ? mqEstablishment.dfeta_name : null,
                    Specialism = specialism?.ToMandatoryQualificationSpecialism(),
                    StartDate = q.dfeta_MQStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    EndDate = q.dfeta_MQ_Date.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    Status = status?.ToMandatoryQualificationStatus()
                };
            });

        return Task.WhenAll(mqs);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        Name = personInfo.Name;
    }

    public record MandatoryQualificationInfo
    {
        public required Guid QualificationId { get; init; }
        public required string? Provider { get; init; }
        public required MandatoryQualificationSpecialism? Specialism { get; init; }
        public required DateOnly? StartDate { get; init; }
        public required DateOnly? EndDate { get; init; }
        public required MandatoryQualificationStatus? Status { get; init; }
    }
}
